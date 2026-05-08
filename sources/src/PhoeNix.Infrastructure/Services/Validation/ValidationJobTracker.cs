using System.Collections.Concurrent;
using System.Threading.Channels;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Infrastructure.Services.Validation;

internal sealed class ValidationJobTracker : IValidationJobTracker
{
    private readonly Channel<ValidationJob> _channel = Channel.CreateUnbounded<ValidationJob>();
    private readonly ConcurrentDictionary<SystemValidationKey, ValidationJobStatus> _systemStatuses = new();
    private readonly ConcurrentDictionary<ModuleValidationKey, ValidationJobStatus> _moduleStatuses = new();
    private readonly ConcurrentDictionary<Guid, string> _materializedPaths = new();

    public ChannelReader<ValidationJob> Reader => _channel.Reader;

    public void EnqueueSystemValidation(ValidationJob job)
    {
        if (job.SystemKey is not null)
            _systemStatuses[job.SystemKey] = new ValidationJobStatus(ValidationJobState.Queued);
        _channel.Writer.TryWrite(job);
    }

    public void EnqueueModuleValidation(ValidationJob job)
    {
        if (job.ModuleKey is not null)
            _moduleStatuses[job.ModuleKey] = new ValidationJobStatus(ValidationJobState.Queued);
        _channel.Writer.TryWrite(job);
    }

    public ValidationJobStatus GetSystemStatus(SystemValidationKey key) =>
        _systemStatuses.TryGetValue(key, out var status)
            ? status
            : new ValidationJobStatus(ValidationJobState.None);

    public ValidationJobStatus GetModuleStatus(ModuleValidationKey key) =>
        _moduleStatuses.TryGetValue(key, out var status)
            ? status
            : new ValidationJobStatus(ValidationJobState.None);

    public void SetSystemStatus(SystemValidationKey key, ValidationJobStatus status) =>
        _systemStatuses[key] = status;

    public void SetModuleStatus(ModuleValidationKey key, ValidationJobStatus status) =>
        _moduleStatuses[key] = status;

    public string? GetMaterializedPath(ConfigurationId configId) =>
        _materializedPaths.TryGetValue(configId.Value, out var path) ? path : null;

    public void SetMaterializedPath(ConfigurationId configId, string path) =>
        _materializedPaths[configId.Value] = path;

    public bool HasActiveJobsForConfiguration(ConfigurationId configId)
    {
        return _systemStatuses.Any(kv =>
                   kv.Key.ConfigurationId == configId &&
                   kv.Value.State is ValidationJobState.Queued or ValidationJobState.Running)
               || _moduleStatuses.Any(kv =>
                   kv.Key.ConfigurationId == configId &&
                   kv.Value.State is ValidationJobState.Queued or ValidationJobState.Running);
    }
}
