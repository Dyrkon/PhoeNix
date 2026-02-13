using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface IConfigurationFilesBuilder
{
    public Result<Folder> BuildConfiguration(ConfigurationBuildResult configurationBuild);
}