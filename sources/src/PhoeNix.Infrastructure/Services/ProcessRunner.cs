using System.Diagnostics;
using System.Text;
using PhoeNix.Domain.Models.Processes;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ProcessRunner : IProcessRunner
{
    public Result<ProcessResult> RunProcess(
        string executableName,
        List<string> arguments,
        CancellationToken cancellationToken,
        string? workingDirectory = null,
        string? standardInput = null,
        DataReceivedEventHandler? perLineAction = null,
        TimeSpan? timeOut = null)
    {
        using var process = new Process();

        var startTimestamp = DateTimeOffset.UtcNow;
        var sw = Stopwatch.StartNew();

        var stdout = new StringBuilder(8 * 1024);
        var stderr = new StringBuilder(8 * 1024);

        var wasCancelled = false;
        var timedOut = false;

        var processInfo = new ProcessStartInfo
        {
            FileName = executableName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            WorkingDirectory = workingDirectory ?? string.Empty
        };

        foreach (var argument in arguments)
            processInfo.ArgumentList.Add(argument);

        process.StartInfo = processInfo;
        process.EnableRaisingEvents = true;

        DataReceivedEventHandler onStdOut = (_, e) =>
        {
            if (e.Data is null) return;
            stdout.AppendLine(e.Data);
            perLineAction?.Invoke(process, e);
        };

        DataReceivedEventHandler onStdErr = (_, e) =>
        {
            if (e.Data is null) return;
            stderr.AppendLine(e.Data);
            perLineAction?.Invoke(process, e);
        };

        process.OutputDataReceived += onStdOut;
        process.ErrorDataReceived += onStdErr;

        try
        {
            process.Start();

            if (!string.IsNullOrEmpty(standardInput)) process.StandardInput.Write(standardInput);
            process.StandardInput.Close();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var ctr = cancellationToken.Register(() =>
            {
                wasCancelled = true;
                TryKillProcessTree(process);
            });

            if (timeOut is { } timeout)
            {
                if (!process.WaitForExit((int)timeout.TotalMilliseconds))
                {
                    timedOut = true;
                    TryKillProcessTree(process);

                    process.WaitForExit();
                }
            }
            else
            {
                process.WaitForExit();
            }

            process.WaitForExit();
        }
        catch (Exception e)
        {
            return Result.Failure<ProcessResult>(new Error("ProcessFailed", e.Message));
        }
        finally
        {
            sw.Stop();

            process.OutputDataReceived -= onStdOut;
            process.ErrorDataReceived -= onStdErr;

            try
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch
            {
                /* ignore */
            }
        }

        return new ProcessResult(
            process.ExitCode,
            stdout.ToString(),
            stderr.ToString(),
            sw.Elapsed,
            wasCancelled,
            timedOut,
            startTimestamp.UtcDateTime
        );
    }

    private static void TryKillProcessTree(Process process)
    {
        try
        {
            if (process.HasExited) return;

            process.Kill(true);
        }
        catch
        {
            // ignored
        }
    }
}