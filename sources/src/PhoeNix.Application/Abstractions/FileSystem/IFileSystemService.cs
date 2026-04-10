using PhoeNix.Application.Models.Files;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.FileSystem;

public interface IFileSystemService
{
    Result<string> GetRootFolder();

    Task<Result<string>> WriteConfigurationToFsAsync(
        Folder configurationFolder,
        ConfigurationId configurationId,
        MachineId? machineId,
        CancellationToken cancellationToken);
}