using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Persistence.Seeding;

internal static class ConfigurationSeedFactory
{
    private const string NixpkgsSource = "github:NixOS/nixpkgs/nixos-25.11";

    private const string PhoenixSource =
        "git+ssh://git@github.com/Dyrkon/PhoeNix";

    public static Result<Configuration> CreateMinimalInstallableExample(UserId ownerId,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var systemId = new SystemId(Guid.NewGuid());
        return Configuration.Create(
                new ConfigurationId(Guid.NewGuid()),
                ownerId,
                "Minimal NixOS Anywhere Example",
                "Minimal bootable NixOS target for nixos-anywhere with Disko, callback, and Prometheus node exporter.")
            .Tap(cfg => cfg.AddInput(NixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(templateIds["TimezoneSync"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixFlakeSettings"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixBuildOptimisation"], true))
            .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "demo-install-target"))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["MinimalBaseSystem"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["DiskoEfiExt4System"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["PrometheusNodeExporter"],
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededValues(cfg, templateIds, options));
    }

    private static void SetSeededValues(Configuration cfg,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["TimezoneSync"]);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixFlakeSettings"]);
        nixFlakeModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                options.NixSubstituters).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                options.NixTrustedPublicKeys).Value
        ]);

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixBuildOptimisation"]);
        nixBuildModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixMaxJobs.ToString(),
                SeedPlaceholders.NixMaxJobs,
                SeedPlaceholders.NixMaxJobs).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixCores.ToString(),
                SeedPlaceholders.NixCores,
                SeedPlaceholders.NixCores).Value
        ]);

        var system = cfg.SystemSpecifications.Single(s => s.Name == "demo-install-target");

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["MinimalBaseSystem"]);
        baseModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.HostName),
                SeedPlaceholders.HostName,
                SeedPlaceholders.HostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.StateVersion),
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                options.RootAuthorizedKeys).Value
        ]);

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["DiskoEfiExt4System"]);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["PrometheusNodeExporter"]);
        prometheusModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.MetricsPort.ToString(),
                SeedPlaceholders.MetricsPort,
                SeedPlaceholders.MetricsPort).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.OpenFirewall ? "true" : "false",
                SeedPlaceholders.OpenFirewall,
                SeedPlaceholders.OpenFirewall).Value
        ]);
    }

    public static Result<Configuration> CreatePhoeNixDeploymentExample(UserId ownerId,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var systemId = new SystemId(Guid.NewGuid());
        return Configuration.Create(
                new ConfigurationId(Guid.NewGuid()),
                ownerId,
                "PhoeNix Deployment Server",
                "PhoeNix application server with integrated NCPS binary cache. Prometheus is provided by the PhoeNix service module.")
            .Tap(cfg => cfg.AddInput(NixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddInput(PhoenixSource, "phoenix"))
            .Tap(cfg => cfg.AddModule(templateIds["TimezoneSync"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixFlakeSettings"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixBuildOptimisation"], true))
            .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "phoenix-server"))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["MinimalBaseSystem"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["DiskoEfiBtrfs"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["PhoeNixService"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["Admin User"],
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededPhoeNixDeploymentValues(cfg, templateIds, options));
    }

    public static Result<Configuration> CreateCacheMachineExample(UserId ownerId,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var systemId = new SystemId(Guid.NewGuid());
        return Configuration.Create(
                new ConfigurationId(Guid.NewGuid()),
                ownerId,
                "Nix Binary Cache Server",
                "Dedicated NCPS binary cache server with Prometheus node exporter for monitoring.")
            .Tap(cfg => cfg.AddInput(NixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(templateIds["TimezoneSync"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixFlakeSettings"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixBuildOptimisation"], true))
            .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "nix-cache"))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["MinimalBaseSystem"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["DiskoEfiBtrfs"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["NcpsCacheServer"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["PrometheusNodeExporter"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["Admin User"],
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededCacheMachineValues(cfg, templateIds, options));
    }

    public static Result<Configuration> CreateGnomeWorkstationExample(UserId ownerId,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var systemId = new SystemId(Guid.NewGuid());
        return Configuration.Create(
                new ConfigurationId(Guid.NewGuid()),
                ownerId,
                "GNOME Office Workstation",
                "Locked-down GNOME desktop workstation using the local NCPS binary cache and SSH key hardening.")
            .Tap(cfg => cfg.AddInput(NixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(templateIds["TimezoneSync"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixFlakeSettings"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixBuildOptimisation"], true))
            .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "gnome-workstation"))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["MinimalBaseSystem"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["DiskoEfiBtrfs"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["GnomeWorkstation"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["NcpsCacheClient"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["SystemHardening"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["PrometheusNodeExporter"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["Regular User"],
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededGnomeWorkstationValues(cfg, templateIds, options));
    }

    public static Result<Configuration> CreateKdeWorkstationExample(UserId ownerId,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var systemId = new SystemId(Guid.NewGuid());
        return Configuration.Create(
                new ConfigurationId(Guid.NewGuid()),
                ownerId,
                "KDE Office Workstation",
                "Locked-down KDE Plasma workstation using the local NCPS binary cache and SSH key hardening.")
            .Tap(cfg => cfg.AddInput(NixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(templateIds["TimezoneSync"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixFlakeSettings"], true))
            .Tap(cfg => cfg.AddModule(templateIds["NixBuildOptimisation"], true))
            .Tap(cfg => cfg.AddSystem(systemId, Architecture.X86Linux, "kde-workstation"))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["MinimalBaseSystem"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["DiskoEfiBtrfs"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["KdeWorkstation"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["NcpsCacheClient"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["SystemHardening"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["PrometheusNodeExporter"],
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(systemId, templateIds["Regular User"],
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededKdeWorkstationValues(cfg, templateIds, options));
    }

    private static void SetSeededPhoeNixDeploymentValues(Configuration cfg,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["TimezoneSync"]);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixFlakeSettings"]);
        nixFlakeModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                options.NixSubstituters).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                options.NixTrustedPublicKeys).Value
        ]);

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixBuildOptimisation"]);
        nixBuildModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixMaxJobs.ToString(),
                SeedPlaceholders.NixMaxJobs,
                SeedPlaceholders.NixMaxJobs).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixCores.ToString(),
                SeedPlaceholders.NixCores,
                SeedPlaceholders.NixCores).Value
        ]);

        var system = cfg.SystemSpecifications.Single(s => s.Name == "phoenix-server");

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["MinimalBaseSystem"]);
        baseModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("phoenix-server"),
                SeedPlaceholders.HostName,
                SeedPlaceholders.HostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.StateVersion),
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                options.RootAuthorizedKeys).Value
        ]);

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["DiskoEfiBtrfs"]);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var phoenixModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["PhoeNixService"]);
        phoenixModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("http://phoenix-server/api"),
                SeedPlaceholders.PhoenixPublicBaseUrl,
                SeedPlaceholders.PhoenixPublicBaseUrl).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("127.0.0.1:5000"),
                SeedPlaceholders.NcpsServerAddress,
                SeedPlaceholders.NcpsServerAddress).Value
        ]);

        var adminUserModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["Admin User"]);
        adminUserModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("admin"),
                SeedPlaceholders.UserName,
                SeedPlaceholders.UserName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("System Administrator"),
                SeedPlaceholders.UserDescription,
                SeedPlaceholders.UserDescription).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserGroups,
                ["\"wheel\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserAuthorizedKeys,
                SeedPlaceholders.UserAuthorizedKeys,
                options.RootAuthorizedKeys).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("changeme"),
                SeedPlaceholders.UserInitialPassword,
                SeedPlaceholders.UserInitialPassword).Value
        ]);
    }

    private static void SetSeededCacheMachineValues(Configuration cfg,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["TimezoneSync"]);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixFlakeSettings"]);
        nixFlakeModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                options.NixSubstituters).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                options.NixTrustedPublicKeys).Value
        ]);

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixBuildOptimisation"]);
        nixBuildModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixMaxJobs.ToString(),
                SeedPlaceholders.NixMaxJobs,
                SeedPlaceholders.NixMaxJobs).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixCores.ToString(),
                SeedPlaceholders.NixCores,
                SeedPlaceholders.NixCores).Value
        ]);

        var system = cfg.SystemSpecifications.Single(s => s.Name == "nix-cache");

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["MinimalBaseSystem"]);
        baseModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("nix-cache"),
                SeedPlaceholders.HostName,
                SeedPlaceholders.HostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.StateVersion),
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                options.RootAuthorizedKeys).Value
        ]);

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["DiskoEfiBtrfs"]);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var cacheServerModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["NcpsCacheServer"]);
        cacheServerModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("nix-cache"),
                SeedPlaceholders.NcpsCacheHostName,
                SeedPlaceholders.NcpsCacheHostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("0.0.0.0:5000"),
                SeedPlaceholders.NcpsServerAddress,
                SeedPlaceholders.NcpsServerAddress).Value
        ]);

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["PrometheusNodeExporter"]);
        prometheusModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.MetricsPort.ToString(),
                SeedPlaceholders.MetricsPort,
                SeedPlaceholders.MetricsPort).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.OpenFirewall,
                SeedPlaceholders.OpenFirewall,
                ["true", "false"],
                "true").Value
        ]);

        var adminUserModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["Admin User"]);
        adminUserModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("admin"),
                SeedPlaceholders.UserName,
                SeedPlaceholders.UserName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("System Administrator"),
                SeedPlaceholders.UserDescription,
                SeedPlaceholders.UserDescription).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserGroups,
                ["\"wheel\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserAuthorizedKeys,
                SeedPlaceholders.UserAuthorizedKeys,
                options.RootAuthorizedKeys).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("changeme"),
                SeedPlaceholders.UserInitialPassword,
                SeedPlaceholders.UserInitialPassword).Value
        ]);
    }

    private static void SetSeededGnomeWorkstationValues(Configuration cfg,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["TimezoneSync"]);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixFlakeSettings"]);
        nixFlakeModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                options.NixSubstituters).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                options.NixTrustedPublicKeys).Value
        ]);

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixBuildOptimisation"]);
        nixBuildModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixMaxJobs.ToString(),
                SeedPlaceholders.NixMaxJobs,
                SeedPlaceholders.NixMaxJobs).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixCores.ToString(),
                SeedPlaceholders.NixCores,
                SeedPlaceholders.NixCores).Value
        ]);

        var system = cfg.SystemSpecifications.Single(s => s.Name == "gnome-workstation");

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["MinimalBaseSystem"]);
        baseModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("gnome-workstation"),
                SeedPlaceholders.HostName,
                SeedPlaceholders.HostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.StateVersion),
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                options.RootAuthorizedKeys).Value
        ]);

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["DiskoEfiBtrfs"]);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var gnomeModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["GnomeWorkstation"]);
        gnomeModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("en_US.UTF-8"),
                SeedPlaceholders.Locale,
                SeedPlaceholders.Locale).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("us"),
                SeedPlaceholders.KeyboardLayout,
                SeedPlaceholders.KeyboardLayout).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.GnomeCoreApps,
                SeedPlaceholders.GnomeCoreApps,
                ["true", "false"],
                "true").Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.GnomeGames,
                SeedPlaceholders.GnomeGames,
                ["true", "false"],
                "false").Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.GnomeDeveloperTools,
                SeedPlaceholders.GnomeDeveloperTools,
                ["true", "false"],
                "false").Value
        ]);

        var cacheClientModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["NcpsCacheClient"]);
        cacheClientModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.LocalCacheSubstituters,
                SeedPlaceholders.LocalCacheSubstituters,
                ["\"http://nix-cache:5000\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.LocalCachePublicKeys,
                SeedPlaceholders.LocalCachePublicKeys,
                ["\"curl http://nix-cache:5000/pubkey\""]).Value
        ]);

        var hardeningModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["SystemHardening"]);
        hardeningModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("admin"),
                SeedPlaceholders.AdminUser,
                SeedPlaceholders.AdminUser).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.SshPermitRootLogin,
                SeedPlaceholders.SshPermitRootLogin,
                ["\"prohibit-password\"", "\"no\""],
                "\"prohibit-password\"").Value
        ]);

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["PrometheusNodeExporter"]);
        prometheusModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.MetricsPort.ToString(),
                SeedPlaceholders.MetricsPort,
                SeedPlaceholders.MetricsPort).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.OpenFirewall,
                SeedPlaceholders.OpenFirewall,
                ["true", "false"],
                "true").Value
        ]);

        var regularUserModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["Regular User"]);
        regularUserModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("user"),
                SeedPlaceholders.UserName,
                SeedPlaceholders.UserName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("Desktop User"),
                SeedPlaceholders.UserDescription,
                SeedPlaceholders.UserDescription).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserGroups,
                ["\"video\"", "\"audio\"", "\"networkmanager\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserAuthorizedKeys,
                SeedPlaceholders.UserAuthorizedKeys,
                []).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("changeme"),
                SeedPlaceholders.UserInitialPassword,
                SeedPlaceholders.UserInitialPassword).Value
        ]);
    }

    private static void SetSeededKdeWorkstationValues(Configuration cfg,
        IReadOnlyDictionary<string, ModuleTemplateId> templateIds, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["TimezoneSync"]);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixFlakeSettings"]);
        nixFlakeModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                options.NixSubstituters).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                options.NixTrustedPublicKeys).Value
        ]);

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == templateIds["NixBuildOptimisation"]);
        nixBuildModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixMaxJobs.ToString(),
                SeedPlaceholders.NixMaxJobs,
                SeedPlaceholders.NixMaxJobs).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixCores.ToString(),
                SeedPlaceholders.NixCores,
                SeedPlaceholders.NixCores).Value
        ]);

        var system = cfg.SystemSpecifications.Single(s => s.Name == "kde-workstation");

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["MinimalBaseSystem"]);
        baseModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("kde-workstation"),
                SeedPlaceholders.HostName,
                SeedPlaceholders.HostName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.StateVersion),
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                options.RootAuthorizedKeys).Value
        ]);

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["DiskoEfiBtrfs"]);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var kdeModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["KdeWorkstation"]);
        kdeModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("en_US.UTF-8"),
                SeedPlaceholders.Locale,
                SeedPlaceholders.Locale).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("us"),
                SeedPlaceholders.KeyboardLayout,
                SeedPlaceholders.KeyboardLayout).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.KdePrinting,
                SeedPlaceholders.KdePrinting,
                ["true", "false"],
                "false").Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.KdeConnect,
                SeedPlaceholders.KdeConnect,
                ["true", "false"],
                "false").Value
        ]);

        var cacheClientModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["NcpsCacheClient"]);
        cacheClientModule.ReplaceEntries(
        [
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.LocalCacheSubstituters,
                SeedPlaceholders.LocalCacheSubstituters,
                ["\"http://nix-cache:5000\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.LocalCachePublicKeys,
                SeedPlaceholders.LocalCachePublicKeys,
                ["\"curl http://nix-cache:5000/pubkey\""]).Value
        ]);

        var hardeningModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["SystemHardening"]);
        hardeningModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("admin"),
                SeedPlaceholders.AdminUser,
                SeedPlaceholders.AdminUser).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.SshPermitRootLogin,
                SeedPlaceholders.SshPermitRootLogin,
                ["\"prohibit-password\"", "\"no\""],
                "\"prohibit-password\"").Value
        ]);

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["PrometheusNodeExporter"]);
        prometheusModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.MetricsPort.ToString(),
                SeedPlaceholders.MetricsPort,
                SeedPlaceholders.MetricsPort).Value,
            SingleChoiceValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.OpenFirewall,
                SeedPlaceholders.OpenFirewall,
                ["true", "false"],
                "true").Value
        ]);

        var regularUserModule = system.Modules.Single(m => m.ModuleTemplateId == templateIds["Regular User"]);
        regularUserModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("user"),
                SeedPlaceholders.UserName,
                SeedPlaceholders.UserName).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("Desktop User"),
                SeedPlaceholders.UserDescription,
                SeedPlaceholders.UserDescription).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserGroups,
                ["\"video\"", "\"audio\"", "\"networkmanager\""]).Value,
            ListValue.Create(
                new EntryValueId(Guid.NewGuid()),
                SeedPlaceholders.UserAuthorizedKeys,
                SeedPlaceholders.UserAuthorizedKeys,
                []).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("changeme"),
                SeedPlaceholders.UserInitialPassword,
                SeedPlaceholders.UserInitialPassword).Value
        ]);
    }

    private static string ToNixString(string value)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }
}