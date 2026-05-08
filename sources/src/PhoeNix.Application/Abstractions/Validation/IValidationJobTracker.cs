using System.Threading.Channels;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Application.Abstractions.Validation;

public interface IValidationJobTracker
{
    void EnqueueSystemValidation(ValidationJob job);
    void EnqueueModuleValidation(ValidationJob job);
    ValidationJobStatus GetSystemStatus(SystemValidationKey key);
    ValidationJobStatus GetModuleStatus(ModuleValidationKey key);
    void SetSystemStatus(SystemValidationKey key, ValidationJobStatus status);
    void SetModuleStatus(ModuleValidationKey key, ValidationJobStatus status);
    string? GetMaterializedPath(ConfigurationId configId);
    void SetMaterializedPath(ConfigurationId configId, string path);
    bool HasActiveJobsForConfiguration(ConfigurationId configId);
    ChannelReader<ValidationJob> Reader { get; }
}
