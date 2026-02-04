using Microsoft.Extensions.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using System.IO.Abstractions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Options;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class FileSystemService(IOptions<FileStorageOptions> storageOptions, INixFormatterService nixFormatterService)
    : IFileSystemService
{
    private const string TempFolderName = "phoenix";

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

    private static Result CheckAndRemoveDirectory(string path)
    {
        var fs = new FileSystem();
        if (fs.Directory.Exists(path)) fs.Directory.Delete(path, true);

        return Result.Success();
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

            return Result.Success(rootPath);
        });
    }

    public Result<string> GetRootFolder()
    {
        return storageOptions.Value.UseTemp
            ? Path.Combine(Path.GetTempPath(), TempFolderName)
            : Path.Combine(storageOptions.Value.RootPath);
    }

    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id)
    {
        return GetRootFolder()
            .Tap(path => CheckAndRemoveDirectory(path))
            .Bind(path => WriteFolderStructure(path, configurationFolder))
            .Bind(nixFormatterService.FormatNixFilesInPlace);
    }
}