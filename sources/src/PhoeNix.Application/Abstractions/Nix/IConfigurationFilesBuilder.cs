using PhoeNix.Application.Models.Files;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface IConfigurationFilesBuilder
{
    public Result<Folder> BuildConfigurationFiles(ConfigurationBuildResult configurationBuild);
}