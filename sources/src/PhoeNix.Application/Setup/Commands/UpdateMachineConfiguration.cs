using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record UpdateMachineConfiguration(
    ConfigurationId ConfigurationId,
    MachineId MachineId,
    SystemId SystemId) : ICommand;

internal sealed class UpdateMachineConfigurationHandler(
    IMachineRepository machineRepository,
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    INixBuildMaterializer nixBuildMaterializer,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    INixOsMachineUpdater nixOsMachineUpdater,
    IDeploySshKeyProvider deploySshKeyProvider,
    IDeploymentBindingResolver deploymentBindingResolver)
    : ICommandHandler<UpdateMachineConfiguration>
{
    public async Task<Result> Handle(
        UpdateMachineConfiguration request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var machineResult = await machineRepository
            .GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{request.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        var machine = machineResult.Value;

        if (!machine.Enabled)
            return Result.Failure(new Error(
                "MachineDisabled",
                $"Machine '{machine.Title}' is disabled."));

        if (machine.DeploymentSnapshot is null)
            return Result.Failure(new Error(
                "MachineDeploymentSnapshotMissing",
                $"Machine '{machine.Title}' does not have a known deployment state."));

        var deploymentSnapshot = machine.DeploymentSnapshot;
        var targetIpAddress = deploymentSnapshot.LastKnownIpAddress;

        var configurationResult = await configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error(
                "ConfigurationNotFound",
                $"Configuration '{request.ConfigurationId.Value}' was not found."));

        if (configurationResult.IsFailure)
            return configurationResult.Error;

        var configuration = configurationResult.Value;

        var selectedSystem = configuration.SystemSpecifications
            .FirstOrDefault(s => s.Id == request.SystemId);

        if (selectedSystem is null)
            return Result.Failure(new Error(
                "SystemNotInConfiguration",
                $"System '{request.SystemId.Value}' is not part of configuration '{configuration.Id.Value}'."));

        if (selectedSystem.Architecture != machine.Architecture)
            return Result.Failure(new Error(
                "MachineArchitectureMismatch",
                $"System '{selectedSystem.Name}' targets architecture '{selectedSystem.Architecture}', but machine '{machine.Title}' has architecture '{machine.Architecture}'."));

        var moduleTemplates = await moduleTemplateRepository.GetAllAsync(cancellationToken);
        if (moduleTemplates is null || !moduleTemplates.Any())
            return Result.Failure(new Error(
                "ModuleTemplatesNotFound",
                "Cannot get module templates."));

        var deployAccessResult = await deploySshKeyProvider.GetOrCreateAsync(
            request.MachineId,
            cancellationToken);

        if (deployAccessResult.IsFailure)
            return deployAccessResult.Error;

        var builtInModules = new BuiltInModuleParameters(
            null,
            new DeployAccessModuleParameters(
                deployAccessResult.Value.DeployUser,
                deployAccessResult.Value.CaPublicKey));

        var bindingResult = deploymentBindingResolver.ApplyBindings(
            configuration,
            moduleTemplates,
            deploymentSnapshot);

        if (bindingResult.IsFailure)
            return bindingResult.Error;

        var materializedResult = nixBuildMaterializer.MaterializeConfiguration(
            bindingResult.Value,
            moduleTemplates,
            request.SystemId,
            builtInModules);

        if (materializedResult.IsFailure)
            return materializedResult.Error;

        var builtFilesResult = configurationFilesBuilder.BuildConfigurationFiles(materializedResult.Value);
        if (builtFilesResult.IsFailure)
            return builtFilesResult.Error;

        var writeResult = await fileSystemService.WriteConfigurationToFsAsync(
            builtFilesResult.Value,
            request.ConfigurationId,
            cancellationToken);

        if (writeResult.IsFailure)
            return writeResult.Error;

        var updateResult = await nixOsMachineUpdater.UpdateAsync(
            targetIpAddress,
            writeResult.Value,
            request.SystemId.ToStringWithPrefix(),
            deployAccessResult.Value,
            cancellationToken);

        if (updateResult.IsFailure)
            return updateResult.Error;

        var boundDiskPaths = deploymentSnapshot.BoundDisks
            .OrderBy(d => d.Index)
            .Select(d => d.StableDevicePath)
            .ToList();

        var snapshotResult = machine.RecordDeploymentSnapshot(
            request.ConfigurationId,
            configuration.Title,
            request.SystemId,
            configuration.SystemSpecifications.First(s => s.Id == request.SystemId).Name,
            targetIpAddress,
            nowUtc,
            boundDiskPaths);

        if (snapshotResult.IsFailure)
            return snapshotResult.Error;

        return Result.Success();
    }
}