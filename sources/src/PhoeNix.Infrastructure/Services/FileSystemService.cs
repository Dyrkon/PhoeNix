using Microsoft.Extensions.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using System.IO.Abstractions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Options;
using PhoeNix.Domain.Service;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class FileSystemService(IOptions<FileStorageOptions> storageOptions) : IFileSystemService
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

    public string GetConfigurationFolderPath(ConfigurationId id)
    {
        return Path.Combine(storageOptions.Value.ConfigurationsPath, id.Value.ToString());
    }

    public string GetModuleFolderPath(ModuleId id)
    {
        return Path.Combine(storageOptions.Value.ModulesPath, id.Value.ToString());
    }

    public string GetTempModuleFolderPath(ModuleId id)
    {
        return Path.Combine(Path.GetTempPath(), TempFolderName, id.Value.ToString());
    }

    public string GetTempConfigurationFolderPath(ConfigurationId id)
    {
        return Path.Combine(Path.GetTempPath(), TempFolderName, id.Value.ToString());
    }

    public Result<string> CreateConfigurationFolder(ConfigurationId id)
    {
        return CreateFolder(GetConfigurationFolderPath(id));
    }

    public Result<string> CreateTempConfigurationFolder(ConfigurationId id)
    {
        return CreateFolder(GetTempConfigurationFolderPath(id));
    }

    public Result<string> CreateTempModuleFolder(ModuleId id)
    {
        return CreateFolder(GetTempModuleFolderPath(id));
    }

    public Result<string> CreateModuleFolder(ModuleId id)
    {
        return CreateFolder(GetModuleFolderPath(id));
    }

    private static Result<string> WriteFolderStructure(string rootPath, Folder folder)
    {
        return CreateFolder(Path.Combine(rootPath, folder.Name)).Tap(path =>
        {
            foreach (var file in folder.Files)
                if (file.IsFolder)
                    WriteFolderStructure(path, (Folder)file);
                else
                    WriteFile(path, ((TextFile)file).Content);
        });
    }

    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id)
    {
        var configFolderPath = GetConfigurationFolderPath(id);
        // TODO if (!fs.Directory.Exists(configFolder))
        return WriteFolderStructure(configFolderPath, configurationFolder);
    }

    public Result<string> WriteModuleToFs(Folder moduleFolder, ModuleId id)
    {
        var moduleFolderPath = GetModuleFolderPath(id);
        // TODO if (!fs.Directory.Exists(configFolder))
        return WriteFolderStructure(moduleFolderPath, moduleFolder);
    }

    public Result<string> WriteModuleToTmp(Folder moduleFolder, ModuleId id)
    {
        var moduleFolderPath = GetTempModuleFolderPath(id);
        // TODO if (!fs.Directory.Exists(configFolder))
        return WriteFolderStructure(moduleFolderPath, moduleFolder);
    }

    public Result<string> WriteConfigurationToTmp(Folder configurationFolder, ConfigurationId id)
    {
        var configFolderPath = GetTempConfigurationFolderPath(id);
        // TODO if (!fs.Directory.Exists(configFolder))
        return WriteFolderStructure(configFolderPath, configurationFolder);
    }
}