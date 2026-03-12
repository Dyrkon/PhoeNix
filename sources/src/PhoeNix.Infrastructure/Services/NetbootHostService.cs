using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class NetbootHostService : INetbootHostService, IDisposable
{
    private readonly NetbootHostOptions _options;
    private readonly object _sync = new();
    private Timer? _monitorTimer;
    private Process? _process;
    private DateTime? _startedAtUtc;

    public NetbootHostService(IOptions<NetbootHostOptions> options)
    {
        _options = options.Value;
    }

    public Task<Result> StartAsync(
        ProvisioningSessionId sessionId,
        BootArtefactDescriptor artefact,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(artefact.KernelLocation) || !File.Exists(artefact.InitRdLocation))
            return Task.FromResult<Result>(Result.Failure(new Error(
                "NetbootHostMissingArtefacts",
                "Kernel or initrd path does not exist.")));

        lock (_sync)
        {
            if (IsRunningLocked())
                return Task.FromResult(Result.Success());

            var args = BuildArguments(artefact);

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _options.HostExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                };

                foreach (var arg in args)
                    processStartInfo.ArgumentList.Add(arg);

                var process = Process.Start(processStartInfo);
                if (process is null)
                    return Task.FromResult<Result>(Result.Failure(new Error(
                        "NetbootHostStartFailed",
                        "Failed to start netboot host process.")));

                _process = process;
                _startedAtUtc = DateTime.UtcNow;
                StartMonitor();

                return Task.FromResult(Result.Success());
            }
            catch (Exception e)
            {
                return Task.FromResult<Result>(Result.Failure(new Error("NetbootHostStartFailed", e.Message)));
            }
        }
    }

    public Task<Result> StopAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_process is null)
                return Task.FromResult(Result.Success());

            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(true);
                    _process.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
                }
            }
            catch (Exception e)
            {
                return Task.FromResult<Result>(Result.Failure(new Error("NetbootHostStopFailed", e.Message)));
            }
            finally
            {
                ClearStateLocked();
            }

            return Task.FromResult(Result.Success());
        }
    }

    public Task<Result<NetbootHostStatus>> GetStatusAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_process is null)
                return Task.FromResult(Result.Success(new NetbootHostStatus(false, null, null, "stopped")));

            _process.Refresh();
            if (_process.HasExited)
            {
                ClearStateLocked();
                return Task.FromResult(Result.Success(new NetbootHostStatus(false, null, null, "stopped")));
            }

            return Task.FromResult(Result.Success(new NetbootHostStatus(
                true,
                _process.Id,
                _startedAtUtc,
                null)));
        }
    }

    private List<string> BuildArguments(BootArtefactDescriptor artefact)
    {
        var args = new List<string>
        {
            "boot",
            artefact.KernelLocation,
            artefact.InitRdLocation,
            "--cmdline",
            artefact.Cmdline,
            "--port",
            _options.Port.ToString(),
            "--status-port",
            _options.StatusPort.ToString(),
            "--dhcp-no-bind",
            "--debug"
        };

        return args;
    }

    private void StartMonitor()
    {
        _monitorTimer?.Dispose();
        _monitorTimer = new Timer(
            _ =>
            {
                lock (_sync)
                {
                    if (_process is null) return;
                    _process.Refresh();
                    if (_process.HasExited) ClearStateLocked();
                }
            },
            null,
            _options.HealthCheckInterval,
            _options.HealthCheckInterval);
    }

    private bool IsRunningLocked()
    {
        if (_process is null) return false;

        _process.Refresh();
        if (_process.HasExited)
        {
            ClearStateLocked();
            return false;
        }

        return true;
    }

    private void ClearStateLocked()
    {
        try
        {
            _process?.Dispose();
        }
        catch
        {
        }

        _process = null;
        _startedAtUtc = null;

        _monitorTimer?.Dispose();
        _monitorTimer = null;
    }

    public void Dispose()
    {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}