using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record AddConfigurationCommand(string Name, string Description) : ICommand<string>;

internal sealed class AddConfigurationCommandHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    IConfigurationRepository configurationRepository
) : ICommandHandler<AddConfigurationCommand, string>
{
    public Task<Result<string>> Handle(AddConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configurationId = new ConfigurationId(new Guid("7e85a62f-ad28-484d-93fd-dc52af305d53"));
        var sharedTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var systemTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var diskoTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var vmDiskoSysId = new SystemId(Guid.NewGuid());

        const string pEnableSteam = "One";
        const string pOpenFirewall = "Two";
        const string pIsContainer = "Three";
        const string pStateVersion = "Four";
        const string pDiskDevice = "DiskDevice";
        const string pVmStateVersion = "VmStateVersion";

        var sharedDefs = new List<EntryValueDefinition>
        {
            new(sharedTemplateId, pEnableSteam, pEnableSteam, UserInputType.Text),
            new(sharedTemplateId, pOpenFirewall, pOpenFirewall, UserInputType.Text)
        };

        var systemDefs = new List<EntryValueDefinition>
        {
            new(systemTemplateId, pDiskDevice, pDiskDevice, UserInputType.Text),
            new(systemTemplateId, pIsContainer, pIsContainer, UserInputType.Text),
            new(systemTemplateId, pStateVersion, pStateVersion, UserInputType.Text)
        };

        var diskoDefs = new List<EntryValueDefinition>
        {
            new(diskoTemplateId, pDiskDevice, pDiskDevice, UserInputType.Text),
            new(diskoTemplateId, pVmStateVersion, pVmStateVersion, UserInputType.Text)
        };

        var sharedTemplate = ModuleTemplate.Create(sharedTemplateId, "SharedModule", true, ModuleType.Generic,
                [Architecture.X86Linux])
            .Tap(t => t.ChangeContent(
                "nixpkgs.config.allowUnfree = true;\n" +
                "programs.steam = {\n" +
                $"  enable = {pEnableSteam};\n" +
                "  remotePlay.openFirewall = true;\n" +
                $"  dedicatedServer.openFirewall = {pOpenFirewall};\n" +
                "};",
                sharedDefs))
            .Value;

        var systemTemplate = ModuleTemplate.Create(systemTemplateId, "SystemModule", true, ModuleType.System,
                [Architecture.X86Linux])
            .Tap(t => t.ChangeContent(
                "boot.loader.grub.enable = true;\n" +
                $"boot.loader.grub.devices = {pDiskDevice};\n" +
                $"boot.isContainer = {pIsContainer};\n" +
                $"system.stateVersion = {pStateVersion};\n" +
                "networking.hostName = \"test-container\";",
                systemDefs))
            .Value;

        var diskoTemplate = ModuleTemplate.Create(diskoTemplateId, "DiskoSystemModule", true, ModuleType.System,
                [Architecture.X86Linux])
            .Tap(t => t.ChangeContent(
                "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
                $"system.stateVersion = {pVmStateVersion};\n" +
                "networking.hostName = \"vm-disko-test\";\n\n" +
                "disko.devices.disk.main = {\n" +
                "  type = \"disk\";\n" +
                $"  device = {pDiskDevice};\n" +
                "  content = {\n" +
                "    type = \"gpt\";\n" +
                "    partitions = {\n" +
                "      biosBoot = { size = \"1M\"; type = \"EF02\"; };\n" +
                "      root = {\n" +
                "        size = \"100%\";\n" +
                "        content = { type = \"filesystem\"; format = \"ext4\"; mountpoint = \"/\"; };\n" +
                "      };\n" +
                "    };\n" +
                "  };\n" +
                "};",
                diskoDefs))
            .Value;

        moduleTemplateRepository.Add(sharedTemplate);
        moduleTemplateRepository.Add(systemTemplate);
        moduleTemplateRepository.Add(diskoTemplate);

        var result =
            Configuration.Create(configurationId, request.Name, request.Description)
                .Tap(cfg =>
                    cfg.AddInput("github:NixOS/nixpkgs/nixos-unstable", "nixpkgs")
                        .Tap(nixpkgs =>
                            cfg.AddInput("github:snowfallorg/flake", "snowfall")
                                .Tap(snowfall => cfg.AddInputFollow(snowfall.Id, nixpkgs.Name, nixpkgs.Name))
                        )
                )
                .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "TestSystem"))
                .Tap(cfg => cfg.AddSystem(vmDiskoSysId, Architecture.X86Linux, "VmDiskoSystem"))
                .Tap(cfg => cfg.AddModule(sharedTemplateId, true))
                .Tap(cfg => cfg.AddSystemModule(systemId, systemTemplateId, true))
                .Tap(cfg => cfg.AddSystemModule(vmDiskoSysId, diskoTemplateId, true))
                .Tap(cfg =>
                {
                    var sharedMv = cfg.Modules.First(m => m.ModuleTemplateId == sharedTemplateId);
                    sharedMv.ChangeValues(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pEnableSteam, pEnableSteam)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pOpenFirewall, pOpenFirewall)
                                .Value
                        },
                        sharedTemplate.Content
                    );

                    var sys = cfg.SystemSpecifications.First(s => s.Id == systemId);
                    var sysMv = sys.Modules.First(m => m.ModuleTemplateId == systemTemplateId);
                    sysMv.ChangeValues(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "[\"/foo/bar\"]", pDiskDevice,
                                pDiskDevice).Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pIsContainer, pIsContainer)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"25.11\"", pStateVersion,
                                pStateVersion).Value
                        },
                        systemTemplate.Content
                    );

                    var vm = cfg.SystemSpecifications.First(s => s.Id == vmDiskoSysId);
                    var diskoMv = vm.Modules.First(m => m.ModuleTemplateId == diskoTemplateId);
                    diskoMv.ChangeValues(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"/dev/vda\"", pDiskDevice, pDiskDevice)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"25.11\"", pVmStateVersion,
                                pVmStateVersion).Value
                        },
                        diskoTemplate.Content
                    );
                })
                .Tap(configurationRepository.Add)
                .Bind(cfg => Result.Success(cfg.Id.Value.ToString()));

        return Task.FromResult(result);
    }
}