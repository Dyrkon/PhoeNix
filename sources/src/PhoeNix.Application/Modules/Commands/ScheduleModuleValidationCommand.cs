using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Commands;

public record ScheduleModuleValidationCommand(
    ConfigurationId ConfigurationId,
    ModuleTemplateId ModuleTemplateId,
    Architecture Architecture) : ICommand;

internal sealed class ScheduleModuleValidationCommandHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    INixBuildMaterializer nixBuildMaterializer,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    IValidationJobTracker jobTracker,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<ScheduleModuleValidationCommand>
{
    public async Task<Result> Handle(ScheduleModuleValidationCommand request, CancellationToken cancellationToken)
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

        var moduleResult = await moduleTemplateRepository
            .GetByIdAsync(request.ModuleTemplateId, cancellationToken)
            .EnsureNotNull(new Error("ModuleNotFound", $"Module '{request.ModuleTemplateId.Value}' not found."));

        if (moduleResult.IsFailure)
            return moduleResult.Error;

        var module = moduleResult.Value;

        var isInConfig =
            config.Modules.Any(m => m.ModuleTemplateId == module.Id) ||
            config.SystemSpecifications.Any(s => s.Modules.Any(m => m.ModuleTemplateId == module.Id));

        if (!isInConfig)
            return Result.Failure(new Error(
                "ModuleNotInConfiguration",
                $"Module '{module.Name}' is not in configuration '{config.Title}'."));

        if (!module.Tests.Any())
            return Result.Failure(new Error(
                "NoTestsInModule",
                $"Module '{module.Name}' does not have any tests."));

        var path = jobTracker.GetMaterializedPath(request.ConfigurationId);
        if (path is null || !Directory.Exists(path) || !jobTracker.HasActiveJobsForConfiguration(request.ConfigurationId))
        {
            var allTemplates = await moduleTemplateRepository.GetAllAsync(userIdResult.Value, cancellationToken);

            var materializedResult = nixBuildMaterializer.MaterializeConfiguration(config, allTemplates);
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

        var key = new ModuleValidationKey(request.ConfigurationId, request.ModuleTemplateId, request.Architecture);
        jobTracker.SetModuleStatus(key, new ValidationJobStatus(ValidationJobState.Queued));

        var moduleTests = new List<(TestId Id, string Name, string CheckAttributeName)>();
        var isShared = config.Modules.Any(m => m.ModuleTemplateId == module.Id);
        if (isShared)
        {
            foreach (var test in module.Tests)
                moduleTests.Add((test.Id, test.Name, test.Id.ToStringWithPrefix()));
        }
        else
        {
            foreach (var system in config.SystemSpecifications.Where(s =>
                         s.Modules.Any(m => m.ModuleTemplateId == module.Id)))
                foreach (var test in module.Tests)
                    moduleTests.Add((test.Id, test.Name,
                        $"{system.Id.ToStringWithPrefix()}-{test.Id.ToStringWithPrefix()}"));
        }

        var job = new ValidationJob(
            Type: ValidationType.Module,
            SystemKey: null,
            ModuleKey: key,
            SystemArchitecture: null,
            ConfigurationPath: path,
            ModuleTests: moduleTests);

        jobTracker.EnqueueModuleValidation(job);

        return Result.Success();
    }
}
