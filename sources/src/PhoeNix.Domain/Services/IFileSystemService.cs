using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IFileSystemService
{
    public Result<string> GetConfigurationFolderPath(ConfigurationId id);
    public Result<string> GetModuleFolderPath(ModuleId id);
    public Result<string> GetTempModuleFolderPath(ModuleId id);
    public Result<string> GetTempConfigurationFolderPath(ConfigurationId id);
    public Result<string> CreateModuleFolder(ModuleId id);
    public Result<string> CreateConfigurationFolder(ConfigurationId id);
    public Result<string> CreateTempConfigurationFolder(ConfigurationId id);
    public Result<string> CreateTempModuleFolder(ModuleId id);
    public Result<string> WriteModuleToFs(Folder moduleFolder, ModuleId id);
    public Result<string> WriteModuleToTmp(Folder moduleFolder, ModuleId id);
    public Result<string> WriteConfigurationToFs(Folder configurationFolder, ConfigurationId id);
    public Result<string> WriteConfigurationToTmp(Folder configurationFolder, ConfigurationId id);
}