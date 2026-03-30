using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Files;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Filesystem;

public class FileSystemService(
    IOptions<FileStorageOptions> storageOptions,
    INixFormatterService nixFormatterService)
    : IFileSystemService
{
    public Result<string> GetRootFolder()
    {
        var options = storageOptions.Value;

        if (options.UseTemp)
            return Result.Success(Path.Combine(Path.GetTempPath(), "phoenix"));

        var rootBase = string.IsNullOrWhiteSpace(options.RootPath)
            ? "/var/lib/phoenix"
            : options.RootPath;

        var fullPath = PathResolver.CombineWithBase(rootBase, options.ConfigurationsPath);
        return Result.Success(fullPath);
    }

    public async Task<Result<string>> WriteConfigurationToFsAsync(
        Folder configurationFolder,
        ConfigurationId id,
        CancellationToken cancellationToken)
    {
        var rootPathResult = GetRootFolder();
        if (rootPathResult.IsFailure)
            return (Result<string>)rootPathResult.Error;

        var rootPath = rootPathResult.Value;
        var configurationPath = Path.Combine(rootPath, id.Value.ToString());

        try
        {
            Directory.CreateDirectory(rootPath);

            if (Directory.Exists(configurationPath))
                Directory.Delete(configurationPath, true);

            Directory.CreateDirectory(configurationPath);

            await WriteFolderContentsAsync(configurationPath, configurationFolder, cancellationToken);

            return nixFormatterService.FormatNixFilesInPlace(configurationPath, cancellationToken);
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