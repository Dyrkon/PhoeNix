using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IFileSystemService
{
    public Result<string> GetRootFolder();

    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id,
        CancellationToken cancellationToken);
}