using System.Diagnostics;
using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Services;

namespace PhoeNix.Infrastructure.Services;

public class NixFormatterService : INixFormatterService
{
    public Result FormatNixInPlace(string path)
    {
        if (!Directory.Exists(path))
            return Result.Failure(new Error("InvalidPath", $"Directory '{path}' does not exist."));

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nix",
                    Arguments = $"fmt \"{path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var stdErr = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
                return Result.Failure(new Error(
                    "NixFmtFailed",
                    $"Formatter exited with code {process.ExitCode}. Error: {stdErr.Trim()}"
                ));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("NixFormatterException", ex.Message));
        }
    }
}