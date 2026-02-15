using System.Diagnostics;
using System.Text.RegularExpressions;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class NixTestRunner(INixErrorParserService nixErrorParserService, IProcessRunner processRunner)
    : INixTestRunner
{
    public Result<ModuleTestResponse> RunModuleTest(TestId id, string testName, Architecture architecture, string path,
        CancellationToken cancellationToken)
    {
        List<string> arguments =
        [
            "build",
            $"{path}#checks.{architecture.ToArchitectureString()}.{id.ToStringWithPrefix()}",
            "-L",
            "--quiet"
        ];

        return processRunner
            .RunProcess("nix", arguments, cancellationToken, timeOut: TimeSpan.FromMinutes(3))
            .Bind(r => nixErrorParserService.ParseModelTestResult(id, testName, r.StandardOutput.Trim(),
                r.ErrorOutput.Trim(), r.ReturnCode));
    }

    public Result<SystemTestResponse> RunSystemTest(SystemId id, Architecture architecture, string path,
        CancellationToken cancellationToken)
    {
        List<string> arguments =
        [
            "-L",
            "--",
            "--flake",
            $"{path}#{id.ToStringWithPrefix()}",
            "--vm-test"
        ];

        var duration = "unspecified";
        var regex = new Regex("test script finished in ([0-9.]+s)", RegexOptions.Compiled);

        return processRunner
            .RunProcess("nixos-anywhere", arguments, cancellationToken, timeOut: TimeSpan.FromMinutes(3),
                perLineAction: s =>
                {
                    if (s == null) return;

                    var match = regex.Match(s);
                    if (match.Success)
                        duration = match.Groups[1].Value;
                })
            .Map(_ => new SystemTestResponse(id, true, duration));
    }
}