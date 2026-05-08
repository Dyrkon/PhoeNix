using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Systems.Commands;

public record ScheduleSystemValidationCommand(ConfigurationId ConfigurationId, SystemId SystemId) : ICommand;

internal sealed class ScheduleSystemValidationCommandHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    INixBuildMaterializer nixBuildMaterializer,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    IValidationJobTracker jobTracker,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<ScheduleSystemValidationCommand>
{
    public async Task<Result> Handle(ScheduleSystemValidationCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var configResult = await configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Ensure(c => c.OwnerId == userIdResult.Value, ConfigurationErrors.NotFound(request.ConfigurationId));

        if (configResult.IsFailure)
            return configResult.Error;

        var config = configResult.Value;

        var system = config.SystemSpecifications.FirstOrDefault(s => s.Id == request.SystemId);
        if (system is null)
            return Result.Failure(new Error(
                "SystemNotFound",
                $"System '{request.SystemId.Value}' not found in configuration '{config.Title}'."));

        var path = jobTracker.GetMaterializedPath(request.ConfigurationId);
        if (path is null || !Directory.Exists(path) || !jobTracker.HasActiveJobsForConfiguration(request.ConfigurationId))
        {
            var moduleTemplates = await moduleTemplateRepository.GetAllAsync(userIdResult.Value, cancellationToken);

            var materializedResult = nixBuildMaterializer.MaterializeConfiguration(config, moduleTemplates);
            if (materializedResult.IsFailure)
                return materializedResult.Error;

            var builtFilesResult = configurationFilesBuilder.BuildConfigurationFiles(materializedResult.Value);
            if (builtFilesResult.IsFailure)
                return builtFilesResult.Error;

            var writeResult = await fileSystemService.WriteConfigurationToFsAsync(
                builtFilesResult.Value, request.ConfigurationId, null, cancellationToken);
            if (writeResult.IsFailure)
                return writeResult.Error;

            path = writeResult.Value;
            jobTracker.SetMaterializedPath(request.ConfigurationId, path);
        }

        var key = new SystemValidationKey(request.ConfigurationId, request.SystemId);
        jobTracker.SetSystemStatus(key, new ValidationJobStatus(ValidationJobState.Queued));

        var job = new ValidationJob(
            Type: ValidationType.System,
            SystemKey: key,
            ModuleKey: null,
            SystemArchitecture: system.Architecture,
            ConfigurationPath: path,
            ModuleTests: null);

        jobTracker.EnqueueSystemValidation(job);

        return Result.Success();
    }
}
