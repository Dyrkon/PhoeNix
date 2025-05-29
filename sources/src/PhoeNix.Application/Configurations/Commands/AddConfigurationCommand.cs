using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record AddConfigurationCommand() : ICommand;

internal sealed class AddConfigurationCommandHandler(
    IModuleRepository moduleRepository,
    IInputRepository inputRepository,
    ISystemRepository systemRepository,
    IConfigurationRepository configurationRepository) : ICommandHandler<AddConfigurationCommand>
{
    public Task<Result> Handle(AddConfigurationCommand request, CancellationToken cancellationToken)
    {
        // TODO
        var configurationId = new ConfigurationId(new Guid("7e85a62f-ad28-484d-93fd-dc52af305d53"));
        var inputId = new InputId(Guid.NewGuid());
        var sharedModuleId = new ModuleId(Guid.NewGuid());
        var systemModuleId = new ModuleId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());

        // TODO there should be exactly one name occurence in the content
        var placeholder1 = "One";
        var placeholder2 = "Two";
        var placeholder3 = "Three";

        var textValue1 = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder1, "true").Value;
        var textValue2 = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder2, "true").Value;
        var textValue3 = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder3, "\"powersave\"").Value;

        var sharedModule = Module.Create(sharedModuleId, "SharedModule", true, ModuleType.Generic,
            [Architecture.X86Linux]).Tap(m =>
            m.ChangeContent(
                $"programs.steam = {{\nenable = {placeholder1};\n" +
                $"remotePlay.openFirewall = true;" +
                $"\ndedicatedServer.openFirewall = {placeholder2};" +
                "\n# gamescopeSession.enable = true; # Enable gamescope\n};",
                [textValue1, textValue2]
            )).Value;

        var systemModule = Module.Create(systemModuleId, "SystemModule", true, ModuleType.System,
            [Architecture.X86Linux]).Tap(m =>
            m.ChangeContent(
                $"powerManagement.cpuFreqGovernor = lib.mkDefault {placeholder3};",
                [textValue3]
            )).Value;

        moduleRepository.Add(sharedModule);
        moduleRepository.Add(systemModule);

        var input = Input.Create(inputId, "url = \"github:NixOS/nixpkgs/nixos-unstable\";", "nixpkgs").Value;
        inputRepository.Add(input);

        var system = Domain.Entities.Systems.System.Create(systemId, Architecture.X86Linux, "TestSystem").Value;
        system.AddModule(systemModule);

        systemRepository.Add(system);

        var configuration = Configuration.Create(configurationId, "ExampleConfiguration", "Example configuration flake")
            .Value;

        configuration.AddInput(inputId);
        configuration.AddSystem(systemId);
        configuration.AddModule(sharedModuleId);

        configurationRepository.Add(configuration);

        return Result.Success();
    }
}