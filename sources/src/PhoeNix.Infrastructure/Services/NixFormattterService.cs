using System.Diagnostics;
using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Services;

namespace PhoeNix.Infrastructure.Services;

public class NixFormatterService : INixFormatterService
{
    public Result<string> FormatNixFilesInPlace(string path)
    {
        if (!Directory.Exists(path))
            return Result.Failure<string>(new Error("InvalidPath", $"Directory '{path}' does not exist."));

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
                return Result.Failure<string>(new Error(
                    "NixFmtFailed",
                    $"Formatter exited with code {process.ExitCode}. Error: {stdErr.Trim()}"
                ));

            return path;
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(new Error("NixFormatterException", ex.Message));
        }
    }
}