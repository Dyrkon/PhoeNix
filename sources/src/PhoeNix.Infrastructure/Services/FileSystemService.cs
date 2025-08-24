using Microsoft.Extensions.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using System.IO.Abstractions;
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

    public Result<string> GetConfigurationFolderPath(ConfigurationId id)
    {
        return Path.Combine(storageOptions.Value.ConfigurationsPath, id.Value.ToString());
    }

    public Result<string> GetModuleFolderPath(ModuleId id)
    {
        return Path.Combine(storageOptions.Value.ModulesPath, id.Value.ToString());
    }

    public Result<string> GetTempModuleFolderPath(ModuleId id)
    {
        return Path.Combine(Path.GetTempPath(), TempFolderName, id.Value.ToString());
    }

    public Result<string> GetTempConfigurationFolderPath(ConfigurationId id)
    {
        var path = Path.Combine(Path.GetTempPath(), TempFolderName, id.Value.ToString());
        return path;
    }

    public Result<string> CreateConfigurationFolder(ConfigurationId id)
    {
        return GetConfigurationFolderPath(id).Tap(path => CreateFolder(path));
    }

    public Result<string> CreateTempConfigurationFolder(ConfigurationId id)
    {
        return GetTempConfigurationFolderPath(id).Tap(path => CreateFolder(path));
    }

    public Result<string> CreateTempModuleFolder(ModuleId id)
    {
        return GetTempModuleFolderPath(id).Tap(path => CreateFolder(path));
    }

    public Result<string> CreateModuleFolder(ModuleId id)
    {
        return GetModuleFolderPath(id).Tap(path => CreateFolder(path));
    }

    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id)
    {
        return GetConfigurationFolderPath(id)
            .Tap(path => CheckAndRemoveDirectory($"{path}/{configurationFolder.Name}"))
            .Bind(path => WriteFolderStructure(path, configurationFolder))
            .Bind(nixFormatterService.FormatNixFilesInPlace);
    }

    public Result<string> WriteModuleToFs(Folder moduleFolder, ModuleId id)
    {
        return GetModuleFolderPath(id)
            .Tap(path => CheckAndRemoveDirectory($"{path}/{moduleFolder.Name}"))
            .Tap(path => WriteFolderStructure(path, moduleFolder));
    }

    public Result<string> WriteModuleToTmp(Folder moduleFolder, ModuleId id)
    {
        return GetTempModuleFolderPath(id)
            .Tap(path => CheckAndRemoveDirectory($"{path}/{moduleFolder.Name}"))
            .Tap(path => WriteFolderStructure(path, moduleFolder));
    }

    public Result<string> WriteConfigurationToTmp(Folder configurationFolder, ConfigurationId id)
    {
        return GetTempConfigurationFolderPath(id)
            .Tap(path => CheckAndRemoveDirectory($"{path}/{configurationFolder.Name}"))
            .Bind(path => WriteFolderStructure(path, configurationFolder))
            .Bind(nixFormatterService.FormatNixFilesInPlace);
    }
}