using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record AddConfigurationCommand(string Title, string Description) : ICommand<string>;

internal sealed class AddConfigurationCommandHandler(
    IConfigurationRepository configurationRepository
) : ICommandHandler<AddConfigurationCommand, string>
{
    public Task<Result<string>> Handle(AddConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configurationId = new ConfigurationId(new Guid("7e85a62f-ad28-484d-93fd-dc52af305d53"));
        var sharedTemplateId = new ModuleTemplateId(new Guid("11111111-1111-1111-1111-111111111111"));
        var systemTemplateId = new ModuleTemplateId(new Guid("22222222-2222-2222-2222-222222222222"));
        var diskoTemplateId = new ModuleTemplateId(new Guid("33333333-3333-3333-3333-333333333333"));

        var systemId = new SystemId(Guid.NewGuid());
        var vmDiskoSysId = new SystemId(Guid.NewGuid());

        const string pEnableSteam = "One";
        const string pOpenFirewall = "Two";
        const string pIsContainer = "Three";
        const string pStateVersion = "Four";
        const string pDiskDevice = "DiskDevice";
        const string pVmStateVersion = "VmStateVersion";

        var result =
            Configuration.Create(configurationId, request.Title, request.Description)
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
                    sharedMv.ChangeEntry(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pEnableSteam, pEnableSteam)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pOpenFirewall, pOpenFirewall)
                                .Value
                        });

                    var sys = cfg.SystemSpecifications.First(s => s.Id == systemId);
                    var sysMv = sys.Modules.First(m => m.ModuleTemplateId == systemTemplateId);
                    sysMv.ChangeEntry(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "[\"/foo/bar\"]", pDiskDevice,
                                pDiskDevice).Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "true", pIsContainer, pIsContainer)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"25.11\"", pStateVersion,
                                pStateVersion).Value
                        });

                    var vm = cfg.SystemSpecifications.First(s => s.Id == vmDiskoSysId);
                    var diskoMv = vm.Modules.First(m => m.ModuleTemplateId == diskoTemplateId);
                    diskoMv.ChangeEntry(
                        new List<EntryValue>
                        {
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"/dev/vda\"", pDiskDevice, pDiskDevice)
                                .Value,
                            TextValue.Create(new EntryValueId(Guid.NewGuid()), "\"25.11\"", pVmStateVersion,
                                pVmStateVersion).Value
                        });
                })
                .Tap(configurationRepository.Add)
                .Bind(cfg => Result.Success(cfg.Id.Value.ToString()));

        return Task.FromResult(result);
    }
}