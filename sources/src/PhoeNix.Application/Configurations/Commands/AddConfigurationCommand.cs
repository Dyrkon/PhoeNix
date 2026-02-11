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

public record AddConfigurationCommand(string Name, string Description) : ICommand<string>;

internal sealed class AddConfigurationCommandHandler(
    IModuleRepository moduleRepository,
    ISystemRepository systemRepository,
    ITestRepository testRepository,
    IConfigurationRepository configurationRepository) : ICommandHandler<AddConfigurationCommand, string>
{
    public Task<Result<string>> Handle(AddConfigurationCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Name: {request.Name}, Description: {request.Description}");

        var configurationId = new ConfigurationId(new Guid("7e85a62f-ad28-484d-93fd-dc52af305d53"));

        var nixpkgsInputId = new InputId(Guid.NewGuid());
        var testInputId = new InputId(Guid.NewGuid());

        var sharedModuleId = new ModuleId(Guid.NewGuid());
        var systemModuleId = new ModuleId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());

        var diskoSystemModuleId = new ModuleId(Guid.NewGuid());
        var vmDiskoSystemId = new SystemId(Guid.NewGuid());

        var placeholder1 = "One";
        var placeholder2 = "Two";
        var placeholder3 = "Three";
        var placeholder4 = "Four";

        var textValue1 = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder1, "true").Value;
        var textValue2 = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder2, "true").Value;
        var valIsContainer = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder3, "true").Value;
        var valStateVersion = TextValue.Create(new EntryValueId(Guid.NewGuid()), placeholder4, "\"25.11\"").Value;

        var placeholderDiskDevice = "DiskDevice";
        var placeholderVmStateVersion = "VmStateVersion";

        var diskDevice = TextValue.Create(
            new EntryValueId(Guid.NewGuid()),
            placeholderDiskDevice,
            "[\"/foo/bar\"]"
        ).Value;

        var valDiskDevice = TextValue.Create(
            new EntryValueId(Guid.NewGuid()),
            placeholderDiskDevice,
            "\"/dev/vda\""
        ).Value;

        var valVmStateVersion = TextValue.Create(
            new EntryValueId(Guid.NewGuid()),
            placeholderVmStateVersion,
            "\"25.11\""
        ).Value;

        var sharedModule = Module.Create(sharedModuleId, "SharedModule", true, ModuleType.Generic,
                [Architecture.X86Linux])
            .Tap(m => m.ChangeContent(
                $"nixpkgs.config.allowUnfree = true;\n" +
                $"programs.steam = {{\n" +
                $"  enable = {placeholder1};\n" +
                $"  remotePlay.openFirewall = true;\n" +
                $"  dedicatedServer.openFirewall = {placeholder2};\n" +
                $"}};",
                [textValue1, textValue2]
            )).Value;

        var systemModule = Module.Create(systemModuleId, "SystemModule", true, ModuleType.System,
                [Architecture.X86Linux])
            .Tap(m => m.ChangeContent(
                "boot.loader.grub.enable = true;\n" +
                $"boot.loader.grub.devices = {placeholderDiskDevice};\n" +
                $"boot.isContainer = {placeholder3};\n" +
                $"system.stateVersion = {placeholder4};\n" +
                $"networking.hostName = \"test-container\";",
                [diskDevice, valIsContainer, valStateVersion]
            )).Value;

        var diskoSystemModule = Module.Create(diskoSystemModuleId, "DiskoSystemModule", true, ModuleType.System,
                [Architecture.X86Linux])
            .Tap(m => m.ChangeContent(
                "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
                $"system.stateVersion = {placeholderVmStateVersion};\n" +
                "networking.hostName = \"vm-disko-test\";\n\n" +
                "disko.devices.disk.main = {\n" +
                "type = \"disk\";\n" +
                $"device = {placeholderDiskDevice};\n" +
                "content = {\n" +
                "type = \"gpt\";\n" +
                "partitions = {\n" +
                "biosBoot = {\n" +
                "size = \"1M\";\n" +
                "type = \"EF02\";\n" +
                "};\n\n" +
                "root = {\n" +
                "size = \"100%\";\n" +
                "content = {\n" +
                "type = \"filesystem\";\n" +
                "format = \"ext4\";\n" +
                "mountpoint = \"/\";\n" +
                "};\n};\n};\n};\n};",
                [valDiskDevice, valVmStateVersion]
            )).Value;

        var sysModuleTest = Test.Create(new TestId(Guid.NewGuid()), "SysModuleTest").Value;
        var shareModuleTest = Test.Create(new TestId(Guid.NewGuid()), "SharedModuleTest").Value;
        var diskoModuleTest = Test.Create(new TestId(Guid.NewGuid()), "DiskoSystemModuleTest").Value;

        sharedModule.AddModuleTest(shareModuleTest.Id);
        systemModule.AddModuleTest(sysModuleTest.Id);
        diskoSystemModule.AddModuleTest(diskoModuleTest.Id);

        testRepository.Add(sysModuleTest);
        testRepository.Add(shareModuleTest);
        testRepository.Add(diskoModuleTest);

        moduleRepository.Add(sharedModule);
        moduleRepository.Add(systemModule);
        moduleRepository.Add(diskoSystemModule);

        var system = Domain.Entities.Systems.System.Create(systemId, Architecture.X86Linux, "TestSystem").Value;
        system.AddModule(systemModule);
        systemRepository.Add(system);

        var vmDiskoSystem = Domain.Entities.Systems.System
            .Create(vmDiskoSystemId, Architecture.X86Linux, "VmDiskoSystem")
            .Value;
        vmDiskoSystem.AddModule(diskoSystemModule);
        systemRepository.Add(vmDiskoSystem);

        return Task.FromResult(Configuration
            .Create(configurationId, "ExampleConfiguration", "Example configuration flake")
            .Tap(
                configuration => configuration.AddInput("github:NixOS/nixpkgs/nixos-unstable", "nixpkgs")
                    .Tap(i => configuration.AddInput("github:snowfallorg/flake", "snowfall")
                        .Tap(iS => configuration.AddInputFollow(iS.Id, i.Name, i.Name))))
            .Tap(configuration => configuration.AddSystem(systemId))
            .Tap(configuration => configuration.AddSystem(vmDiskoSystemId))
            .Tap(configuration => configuration.AddModule(sharedModuleId))
            .Tap(configurationRepository.Add)
            .Bind(conf => Result.Success(conf.Id.Value.ToString())));
    }
}