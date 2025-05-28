using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Service;

public interface IFileSystemService
{
    public string GetConfigurationFolderPath(ConfigurationId id);
    public string GetModuleFolderPath(ModuleId id);
    public string GetTempModuleFolderPath(ModuleId id);
    public string GetTempConfigurationFolderPath(ConfigurationId id);
    public Result<string> CreateModuleFolder(ModuleId id);
    public Result<string> CreateConfigurationFolder(ConfigurationId id);
    public Result<string> CreateTempConfigurationFolder(ConfigurationId id);
    public Result<string> CreateTempModuleFolder(ModuleId id);
    public Result<string> WriteModuleToFs(Folder moduleFolder, ModuleId id);
    public Result<string> WriteModuleToTmp(Folder moduleFolder, ModuleId id);
    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id);
    public Result<string> WriteConfigurationToTmp(Folder configurationFolder, ConfigurationId id);
}