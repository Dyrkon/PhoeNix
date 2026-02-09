using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IConfigurationTestRunnerService
{
    public Result<ModuleTestResponse> RunModuleTest(TestId id, string testName, Architecture architecture, string path);

    public Result<SystemTestResponse> RunSystemTest(SystemId id, Architecture architecture, string path);
}