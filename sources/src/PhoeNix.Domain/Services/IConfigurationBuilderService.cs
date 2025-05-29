using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IConfigurationBuilderService
{
    public Result<Folder> BuildConfiguration(ConfigurationBuildResult configurationBuild);
}