using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Files;
using PhoeNix.Application.Options;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Filesystem;

public class FileSystemService : IFileSystemService
{
    private readonly bool _useTemp;
    private readonly string _rootPath;
    private readonly string _configurationsPath;
    private readonly INixFormatterService _nixFormatterService;

    public FileSystemService(
        IOptions<FileStorageOptions> storageOptions,
        IAppSettingsRepository settingsRepository,
        INixFormatterService nixFormatterService)
    {
        var options = storageOptions.Value;
        var settings = settingsRepository.GetAsync().GetAwaiter().GetResult();

        _useTemp = options.UseTemp;
        _rootPath = settings is not null && !string.IsNullOrWhiteSpace(settings.FileStorageRootPath)
            ? settings.FileStorageRootPath
            : (string.IsNullOrWhiteSpace(options.RootPath) ? "/var/lib/phoenix" : options.RootPath);
        _configurationsPath = options.ConfigurationsPath;
        _nixFormatterService = nixFormatterService;
    }

    public Result<string> GetRootFolder()
    {
        if (_useTemp)
            return Result.Success(Path.Combine(Path.GetTempPath(), "phoenix"));

        var fullPath = PathResolver.CombineWithBase(_rootPath, _configurationsPath);
        return Result.Success(fullPath);
    }

    public async Task<Result<string>> WriteConfigurationToFsAsync(
        Folder configurationFolder,
        ConfigurationId configurationId,
        MachineId? machineId,
        CancellationToken cancellationToken)
    {
        var rootPathResult = GetRootFolder();
        if (rootPathResult.IsFailure)
            return (Result<string>)rootPathResult.Error;

        var rootPath = rootPathResult.Value;
        var configurationPath = machineId is null
            ? Path.Combine(rootPath, configurationId.Value.ToString())
            : Path.Combine(rootPath, configurationId.Value.ToString(), machineId!.Value.ToString());

        try
        {
            Directory.CreateDirectory(rootPath);

            if (Directory.Exists(configurationPath))
                Directory.Delete(configurationPath, true);

            Directory.CreateDirectory(configurationPath);

            await WriteFolderContentsAsync(configurationPath, configurationFolder, cancellationToken);

            return _nixFormatterService.FormatNixFilesInPlace(configurationPath, cancellationToken);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("WriteConfigurationToFsFailed", e.Message));
        }
    }

    private static async Task WriteFolderContentsAsync(
        string rootPath,
        Folder folder,
        CancellationToken cancellationToken)
    {
        foreach (var file in folder.Files)
        {
            var path = Path.Combine(rootPath, file.Name);

            if (file.IsFolder)
            {
                Directory.CreateDirectory(path);
                await WriteFolderContentsAsync(path, (Folder)file, cancellationToken);
                continue;
            }

            await File.WriteAllTextAsync(path, ((TextFile)file).Content, cancellationToken);
        }
    }
}
