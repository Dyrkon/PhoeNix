using System.Diagnostics;
using System.Text.RegularExpressions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ConfigurationTestRunnerService(INixErrorParserService nixErrorParserService)
    : IConfigurationTestRunnerService
{
    public Result<ModuleTestResponse> RunModuleTest(TestId id, string testName, Architecture architecture, string path)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nix",
                    Arguments =
                        $"build {path}#checks.{architecture.ToArchitectureString()}.{id.ToStringWithPrefix()} -L --quiet",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var stdErr = process.StandardError.ReadToEnd();
            var stdOut = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return nixErrorParserService.ParseModelTestResult(id, testName, stdOut.Trim(), stdErr.Trim(),
                process.ExitCode);
        }
        catch (Exception ex)
        {
            return Result.Failure<ModuleTestResponse>(new Error("NixModuleTestException", ex.Message));
        }
    }

    public Result<SystemTestResponse> RunSystemTest(SystemId id, Architecture architecture, string path)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nixos-anywhere",
                Arguments = $"-L -- --flake {path}#{id.ToStringWithPrefix()} --vm-test",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = psi;

            string? duration = null;

            var regex = new Regex("test script finished in ([0-9.]+s)", RegexOptions.Compiled);

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;

                var match = regex.Match(e.Data);
                if (match.Success)
                    duration = match.Groups[1].Value;
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            if (process.ExitCode != 0)
                return Result.Failure<SystemTestResponse>(
                    new Error("SystemTestFailed", $"Exit code {process.ExitCode}")
                );

            return duration is null
                ? Result.Success(new SystemTestResponse(id, true, "unspecified"))
                : Result.Success(new SystemTestResponse(id, true, duration));
        }
        catch (Exception ex)
        {
            return Result.Failure<SystemTestResponse>(
                new Error("NixOSAnywhereSystemTestException", ex.Message)
            );
        }
    }
}