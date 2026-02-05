using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IConfigurationTestRunnerService
{
    public Result RunModuleTest(string name, Architecture architecture, string path);

    public Result<bool> RunSystemTest(string name, Architecture architecture, string path);
}