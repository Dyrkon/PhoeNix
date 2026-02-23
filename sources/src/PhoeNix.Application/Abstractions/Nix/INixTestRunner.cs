using PhoeNix.Application.Models.Tests;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixTestRunner
{
    public Result<ModuleTestResponse> RunModuleTest(TestId id, string testName, Architecture architecture, string path,
        CancellationToken cancellationToken);

    public Result<SystemTestResponse> RunSystemTest(SystemId id, Architecture architecture, string path,
        CancellationToken cancellationToken);
}