using Microsoft.Extensions.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using System.IO.Abstractions;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Files;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Options;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class FileSystemService(IOptions<FileStorageOptions> storageOptions, INixFormatterService nixFormatterService)
    : IFileSystemService
{
    private static Result<string> CreateFolder(string path)
    {
        if (Directory.Exists(path))
            return Result.Failure<string>(new Error("", $"Folder {path} already exists."));

        try
        {
            var fs = new FileSystem();
            fs.Directory.CreateDirectory(path);
            return path;
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("", e.Message));
        }
    }

    private static Result<string> CheckAndRemoveDirectory(string path)
    {
        var fs = new FileSystem();
        if (fs.Directory.Exists(path)) fs.Directory.Delete(path, true);

        return path;
    }

    private static Result<string> WriteFile(string path, string contents)
    {
        if (Directory.Exists(path))
            return Result.Failure<string>(new Error("", $"File {path} already exists."));

        try
        {
            var fs = new FileSystem();
            fs.File.WriteAllText(path, contents);
            return path;
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("", e.Message));
        }
    }

    private static Result<string> WriteFolderStructure(string rootPath, Folder folder)
    {
        return CreateFolder(Path.Combine(rootPath, folder.Name)).Bind(path =>
        {
            foreach (var file in folder.Files)
                if (file.IsFolder)
                {
                    var result = WriteFolderStructure(path, (Folder)file);
                    if (result.IsFailure) return result;
                }
                else
                {
                    var result = WriteFile($"{path}/{file.Name}", ((TextFile)file).Content);
                    if (result.IsFailure) return result;
                }

            return Result.Success(folder.Name);
        });
    }

    public Result<string> GetRootFolder()
    {
        var options = storageOptions.Value;

        if (options.UseTemp)
            return Path.Combine(Path.GetTempPath(), "phoenix");

        var rootBase = PathResolver.ResolveToHome(options.RootPath);
        var fullPath = PathResolver.CombineWithBase(rootBase, options.ConfigurationsPath);
        return Result.Success(fullPath);
    }

    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id,
        CancellationToken cancellationToken)
    {
        var rootPath = GetRootFolder().Value;
        return CheckAndRemoveDirectory(rootPath)
            .Bind(path => WriteFolderStructure(path, configurationFolder))
            .Bind(path => nixFormatterService.FormatNixFilesInPlace($"{rootPath}/{path}", cancellationToken));
    }
}
