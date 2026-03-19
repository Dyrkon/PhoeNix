using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Commands;

public record AddModuleTemplateCommand(
    string Name,
    bool Enabled,
    ModuleType Type,
    List<Architecture> Architectures) : ICommand;

internal sealed class AddModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository) : ICommandHandler<AddModuleTemplateCommand>
{
    public Task<Result> Handle(AddModuleTemplateCommand request, CancellationToken cancellationToken)
    {
        var sharedTemplateId = new ModuleTemplateId(new Guid("11111111-1111-1111-1111-111111111111"));
        var systemTemplateId = new ModuleTemplateId(new Guid("22222222-2222-2222-2222-222222222222"));
        var diskoTemplateId = new ModuleTemplateId(new Guid("33333333-3333-3333-3333-333333333333"));

        const string pEnableSteam = "One";
        const string pOpenFirewall = "Two";
        const string pIsContainer = "Three";
        const string pStateVersion = "Four";
        const string pDiskDevice = "DiskDevice";
        const string pVmStateVersion = "VmStateVersion";

        var sharedDefs = new List<EntryValueDefinition>
        {
            new(sharedTemplateId, pEnableSteam, pEnableSteam, UserInputType.Text, EntryBindingKind.UserProvided),
            new(sharedTemplateId, pOpenFirewall, pOpenFirewall, UserInputType.Text, EntryBindingKind.UserProvided)
        };

        var sharedTemplateResult =
            ModuleTemplate.Create(sharedTemplateId, "SharedModule", true, ModuleType.Generic, [Architecture.X86Linux])
                .Tap(t => t.ChangeContent(
                    "nixpkgs.config.allowUnfree = true;\n" +
                    "programs.steam = {\n" +
                    $"  enable = {pEnableSteam};\n" +
                    "  remotePlay.openFirewall = true;\n" +
                    $"  dedicatedServer.openFirewall = {pOpenFirewall};\n" +
                    "};",
                    sharedDefs))
                .Tap(t => t.AddModuleTest("shared-steam-test"))
                .Tap(t =>
                {
                    var test = t.Tests.Single(x => x.Name == "shared-steam-test");
                    var testContent =
                        "enableSteam = { expr = One == true; expected = true; };\n" +
                        "openFirewall = { expr = Two == true; expected = true; };";
                    t.ChangeModuleTest(test.Id, testContent, new List<string> { pEnableSteam, pOpenFirewall });
                });

        if (sharedTemplateResult.IsFailure)
            return Task.FromResult<Result>(sharedTemplateResult.Error);

        var systemDefs = new List<EntryValueDefinition>
        {
            new(systemTemplateId, pDiskDevice, pDiskDevice, UserInputType.Text, EntryBindingKind.UserProvided),
            new(systemTemplateId, pIsContainer, pIsContainer, UserInputType.Text, EntryBindingKind.UserProvided),
            new(systemTemplateId, pStateVersion, pStateVersion, UserInputType.Text, EntryBindingKind.UserProvided)
        };

        var systemTemplateResult =
            ModuleTemplate.Create(systemTemplateId, "SystemModule", true, ModuleType.System, [Architecture.X86Linux])
                .Tap(t => t.ChangeContent(
                    "boot.loader.grub.enable = true;\n" +
                    $"boot.loader.grub.devices = {pDiskDevice};\n" +
                    $"boot.isContainer = {pIsContainer};\n" +
                    $"system.stateVersion = {pStateVersion};\n" +
                    "networking.hostName = \"test-container\";",
                    systemDefs))
                .Tap(t => t.AddModuleTest("system-basics-test"))
                .Tap(t =>
                {
                    var test = t.Tests.Single(x => x.Name == "system-basics-test");
                    var testContent =
                        "diskDeviceSet = { expr = DiskDevice != null; expected = true; };\n" +
                        "isContainer = { expr = Three == true; expected = true; };\n" +
                        "stateVersion = { expr = Four == \"25.11\"; expected = true; };";
                    t.ChangeModuleTest(test.Id, testContent,
                        new List<string> { pDiskDevice, pIsContainer, pStateVersion });
                });

        if (systemTemplateResult.IsFailure)
            return Task.FromResult<Result>(systemTemplateResult.Error);

        var diskoDefs = new List<EntryValueDefinition>
        {
            new(diskoTemplateId, pDiskDevice, pDiskDevice, UserInputType.Text, EntryBindingKind.RankedDiskCandidate, 0),
            new(diskoTemplateId, pVmStateVersion, pVmStateVersion, UserInputType.Text, EntryBindingKind.UserProvided)
        };

        var diskoTemplateResult =
            ModuleTemplate.Create(diskoTemplateId, "DiskoSystemModule", true, ModuleType.System,
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
                .Tap(t => t.AddModuleTest("disko-test"))
                .Tap(t =>
                {
                    var test = t.Tests.Single(x => x.Name == "disko-test");
                    var testContent =
                        "disk = { expr = DiskDevice == \"/dev/vda\"; expected = true; };\n" +
                        "state = { expr = VmStateVersion == \"25.11\"; expected = true; };";
                    t.ChangeModuleTest(test.Id, testContent, new List<string> { pDiskDevice, pVmStateVersion });
                });

        if (diskoTemplateResult.IsFailure)
            return Task.FromResult<Result>(diskoTemplateResult.Error);

        moduleTemplateRepository.Add(sharedTemplateResult.Value);
        moduleTemplateRepository.Add(systemTemplateResult.Value);
        moduleTemplateRepository.Add(diskoTemplateResult.Value);

        return Task.FromResult(Result.Success());
    }
}