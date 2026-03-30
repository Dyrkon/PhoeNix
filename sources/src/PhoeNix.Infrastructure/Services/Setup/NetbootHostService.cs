using System.Diagnostics;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Setup;

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

    public Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (IsRunningLocked())
                return Task.FromResult(Result.Success());

            var args = BuildArguments();

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _options.HostExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                foreach (var arg in args)
                    processStartInfo.ArgumentList.Add(arg);

                var process = Process.Start(processStartInfo);
                if (process is null)
                    return Task.FromResult<Result>(Result.Failure(new Error(
                        "NetbootHostStartFailed",
                        "Failed to start netboot host process.")));

                Thread.Sleep(750);
                process.Refresh();

                if (process.HasExited)
                {
                    var stdout = process.StandardOutput.ReadToEnd();
                    var stderr = process.StandardError.ReadToEnd();
                    var exitCode = process.ExitCode;
                    process.Dispose();

                    return Task.FromResult<Result>(Result.Failure(new Error(
                        "NetbootHostStartFailed",
                        $"Pixiecore exited immediately with code {exitCode}. Stdout: {stdout} Stderr: {stderr}")));
                }

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

    private List<string> BuildArguments()
    {
        return new List<string>
        {
            "api",
            _options.ApiBaseUrl.TrimEnd('/'),
            "--port",
            _options.Port.ToString(),
            "--status-port",
            _options.StatusPort.ToString(),
            "--dhcp-no-bind",
            "--debug"
        };
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