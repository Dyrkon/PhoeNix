using System.Diagnostics;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ConfigurationTestRunnerService : IConfigurationTestRunnerService
{
    public Result<bool> RunModuleTest(string name, Architecture architecture)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nix",
                    Arguments = $"build .#checks.{architecture.ToArchitectureString()}.{name} -L",
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
                return Result.Failure<bool>(new Error(
                    "ModuleTestFailed",
                    $"Module test exited with code {process.ExitCode}. Error: {stdErr.Trim()}"
                ));

            return true;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Error("NixModuleTestException", ex.Message));
        }
    }

    public Result<bool> RunSystemTest(string name, Architecture architecture, string path)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nixos-anywhere",
                    Arguments = $"-- --flake {path}#{name} --vm-test",
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
                return Result.Failure<bool>(new Error(
                    "SystemTestFailed",
                    $"System test exited with code {process.ExitCode}. Error: {stdErr.Trim()}"
                ));

            return true;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Error("NixOSAnywhereSystemTestException", ex.Message));
        }
    }
}