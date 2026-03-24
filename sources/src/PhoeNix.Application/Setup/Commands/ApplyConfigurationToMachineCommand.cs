using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Application.Setup;
using PhoeNix.Application.Setup.Extensions;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record ApplyConfigurationToMachineCommand(MachineId MachineId) : ICommand;

internal sealed class ApplyConfigurationToMachineCommandHandler(
    IConfigurationRepository configurationRepository,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    INixBuildMaterializer nixBuildMaterializer,
    IModuleTemplateRepository moduleTemplateRepository,
    ISetupSessionRepository sessionRepository,
    INixosInstaller nixosInstaller,
    IRuntimeBindingResolver runtimeBindingResolver,
    IOptions<NetbootHostOptions> setupCallbackOptions)
    : ICommandHandler<ApplyConfigurationToMachineCommand>
{
    public async Task<Result> Handle(
        ApplyConfigurationToMachineCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var sessionResult = await sessionRepository
            .GetWithEnrolledMachineAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "SessionWithMachineNotFound",
                $"Cannot find setup session with machine '{request.MachineId.Value}'."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;
        var target = session.Targets.First(t => t.MachineId == request.MachineId);

        if (target.Stage != SetupStage.Probed)
        {
            var error = new Error(
                "SetupTargetInvalidStage",
                $"Machine '{request.MachineId.Value}' must be in '{SetupStage.Probed}' stage before configuration can be applied.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        if (target.SelectedConfigurationId is null)
        {
            var error = new Error(
                "MachineConfigurationMissing",
                $"Configuration is not assigned to machine '{request.MachineId.Value}'.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        if (target.SelectedSystemId is null)
        {
            var error = new Error(
                "MachineSystemMissing",
                $"System is not assigned to machine '{request.MachineId.Value}'.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        if (target.CallbackToken is null)
        {
            var error = new Error(
                "SetupCallbackTokenMissing",
                "No callback token is assigned to the setup target.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        var moduleTemplates = await moduleTemplateRepository.GetAllAsync(cancellationToken);
        if (moduleTemplates is null || !moduleTemplates.Any())
        {
            var error = new Error(
                "ModuleTemplatesNotFound",
                "Cannot get module templates.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        var configurationResult = await configurationRepository
            .GetByIdAsync(target.SelectedConfigurationId, cancellationToken)
            .EnsureNotNull(new Error(
                "ConfigurationNotFound",
                $"Configuration '{target.SelectedConfigurationId.Value}' was not found."));

        if (configurationResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                configurationResult.Error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);

        var configuration = configurationResult.Value;

        var systemExists = configuration.SystemSpecifications.Any(s => s.Id == target.SelectedSystemId);
        if (!systemExists)
        {
            var error = new Error(
                "SelectedSystemNotInConfiguration",
                $"System '{target.SelectedSystemId.Value}' is not part of configuration '{configuration.Id.Value}'.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);
        }

        var finalizeUrl = $"{setupCallbackOptions.Value.ApiBasePublicUrl.TrimEnd('/')}/setup/finalize";

        var builtInModules = new BuiltInModuleParameters(
            new CallbackModuleParameters(
                finalizeUrl,
                target.CallbackToken.Token));

        var boundConfigurationResult = runtimeBindingResolver.ApplyBindings(
            configuration,
            moduleTemplates,
            target);

        if (boundConfigurationResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                boundConfigurationResult.Error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc);

        var installResult = await nixBuildMaterializer
            .MaterializeConfiguration(
                boundConfigurationResult.Value,
                moduleTemplates,
                target.SelectedSystemId,
                builtInModules)
            .Bind(configurationFilesBuilder.BuildConfigurationFiles)
            .Bind(files => fileSystemService.WriteConfigurationToFs(
                files,
                target.SelectedConfigurationId,
                cancellationToken))
            .Bind(path => nixosInstaller.InstallAsync(
                session,
                target,
                path,
                target.SelectedSystemId.ToStringWithPrefix(),
                cancellationToken));

        if (installResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                installResult.Error,
                nameof(ApplyConfigurationToMachineCommandHandler),
                nowUtc,
                SetupStage.Failed);

        return session.UpdateMachineStage(
            request.MachineId,
            SetupStage.Orchestrated,
            nowUtc);
    }
}