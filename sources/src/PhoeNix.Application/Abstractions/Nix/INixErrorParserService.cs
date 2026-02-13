using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixErrorParserService
{
    public Result<ModuleTestResponse> ParseModelTestResult(TestId id, string testName, string testOutput,
        string errorOutput, int exitCode);
}