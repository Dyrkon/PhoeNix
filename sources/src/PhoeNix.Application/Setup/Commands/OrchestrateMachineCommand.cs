using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record OrchestrateMachineCommand(MachineId MachineId) : ICommand;

internal sealed class OrchestrateMachineCommandHandler(
    IConfigurationRepository configurationRepository,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    INixBuildMaterializer nixBuildMaterializer,
    IModuleTemplateRepository moduleTemplateRepository,
    ISetupSessionRepository sessionRepository,
    INixosInstaller nixosInstaller,
    IRuntimeBindingResolver runtimeBindingResolver,
    ICallbackTokenService callbackTokenService,
    IOptions<NetbootHostOptions> setupCallbackOptions,
    IOptions<JwtCallbackTokenOptions> tokenOptions,
    ILogger<OrchestrateMachineCommandHandler> logger) : ICommandHandler<OrchestrateMachineCommand>
{
    public async Task<Result> Handle(OrchestrateMachineCommand request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var sessionResult = await sessionRepository.GetWithEnrolledMachineAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "SessionWithMachineNotFound",
                $"Cannot find session with machine {request.MachineId}"));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;
        var target = session.Targets.First(t => t.MachineId.Equals(request.MachineId));

        var configurationId = target.SelectedConfigurationId;
        if (configurationId is null)
            return new Error(
                "MachineConfigurationMissing",
                $"Configuration not assigned to {request.MachineId}");

        var moduleTemplates = await moduleTemplateRepository.GetAllAsync(cancellationToken);
        if (moduleTemplates is null || !moduleTemplates.Any())
            return Result.Failure(new Error(
                "ModuleTemplatesNotFound",
                "Cannot get module templates"));

        var configurationResult = await configurationRepository.GetByIdAsync(configurationId, cancellationToken)
            .EnsureNotNull(new Error(
                "ConfigurationNotFound",
                $"Configuration {configurationId} not found."));

        if (configurationResult.IsFailure)
            return configurationResult.Error;

        var configuration = configurationResult.Value;

        var systemExists = configuration.SystemSpecifications.Any(s => s.Id == target.SelectedSystemId);
        if (!systemExists)
            return Result.Failure(new Error(
                "SelectedSystemNotInConfiguration",
                $"System {target.SelectedSystemId} is not part of configuration."));

        if (target.CallbackToken is not null)
        {
            var clearTokenResult = session.ClearCallbackToken(request.MachineId);
            if (clearTokenResult.IsFailure)
                return clearTokenResult.Error;
        }

        var callbackTokenResult = callbackTokenService.Create(
            session.Id,
            request.MachineId,
            nowUtc,
            tokenOptions.Value.MaxTtl ?? TimeSpan.FromHours(1));

        if (callbackTokenResult.IsFailure)
            return callbackTokenResult.Error;

        var assignTokenResult = session.AssignMachineCallbackToken(request.MachineId, callbackTokenResult.Value);
        if (assignTokenResult.IsFailure)
            return assignTokenResult.Error;

        var finalizeUrl = $"{setupCallbackOptions.Value.ApiBasePublicUrl.TrimEnd('/')}/setup/finalize";

        var builtInModules = new BuiltInModuleParameters(
            new CallbackModuleParameters(
                finalizeUrl,
                callbackTokenResult.Value.Token));

        var boundConfigurationResult =
            runtimeBindingResolver.ApplyBindings(configuration, moduleTemplates, target);

        if (boundConfigurationResult.IsFailure)
            return boundConfigurationResult.Error;

        var boundConfiguration = boundConfigurationResult.Value;

        return await nixBuildMaterializer
            .MaterializeConfiguration(
                boundConfiguration,
                moduleTemplates,
                target.SelectedSystemId,
                builtInModules)
            .Bind(configurationFilesBuilder.BuildConfigurationFiles)
            .Bind(cFolder =>
                fileSystemService.WriteConfigurationToFs(cFolder, configurationId, cancellationToken))
            .Bind(path =>
                nixosInstaller.InstallAsync(
                    session,
                    target,
                    path,
                    target.SelectedSystemId?.ToStringWithPrefix() ?? "default",
                    cancellationToken));
    }
}