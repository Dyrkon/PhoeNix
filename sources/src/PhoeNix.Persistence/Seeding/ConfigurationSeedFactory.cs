using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Persistence.Seeding;

internal static class ConfigurationSeedFactory
{
    public static Result<Configuration> CreateMinimalInstallableExample(SeedExampleOptions options)
    {
        const string nixpkgsSource = "github:NixOS/nixpkgs/nixos-unstable";

        return Configuration.Create(
                SeedIds.ExampleConfiguration,
                "Minimal NixOS Anywhere Example",
                "Minimal bootable NixOS target for nixos-anywhere with Disko, callback, and Prometheus node exporter.")
            .Tap(cfg => cfg.AddInput(nixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(SeedIds.TimezoneSyncTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixFlakeSettingsTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixBuildOptimisationTemplate, true))
            .Tap(cfg => cfg.AddSystem(SeedIds.ExampleSystem, Architecture.X86Linux, "demo-install-target"))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.ExampleSystem, SeedIds.MinimalBaseTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.ExampleSystem, SeedIds.DiskoEfiExt4Template,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.ExampleSystem, SeedIds.PrometheusTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededValues(cfg, options));
    }

    private static void SetSeededValues(Configuration cfg, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.TimezoneSyncTemplate);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixFlakeSettingsTemplate);
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

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixBuildOptimisationTemplate);
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

        var system = cfg.SystemSpecifications.Single(s => s.Id == SeedIds.ExampleSystem);

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.MinimalBaseTemplate);
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

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.DiskoEfiExt4Template);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.PrometheusTemplate);
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

    public static Result<Configuration> CreatePhoeNixDeploymentExample(SeedExampleOptions options)
    {
        const string nixpkgsSource = "github:NixOS/nixpkgs/nixos-unstable";

        return Configuration.Create(
                SeedIds.PhoeNixDeploymentConfiguration,
                "PhoeNix Deployment Server",
                "PhoeNix application server with integrated NCPS binary cache. Prometheus is provided by the PhoeNix service module.")
            .Tap(cfg => cfg.AddInput(nixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(SeedIds.TimezoneSyncTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixFlakeSettingsTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixBuildOptimisationTemplate, true))
            .Tap(cfg => cfg.AddSystem(SeedIds.PhoeNixDeploymentSystem, Architecture.X86Linux, "phoenix-server"))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.PhoeNixDeploymentSystem, SeedIds.MinimalBaseTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.PhoeNixDeploymentSystem, SeedIds.DiskoEfiBtrfsTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.PhoeNixDeploymentSystem, SeedIds.PhoeNixServiceTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.PhoeNixDeploymentSystem, SeedIds.AdminUserTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededPhoeNixDeploymentValues(cfg, options));
    }

    public static Result<Configuration> CreateCacheMachineExample(SeedExampleOptions options)
    {
        const string nixpkgsSource = "github:NixOS/nixpkgs/nixos-unstable";

        return Configuration.Create(
                SeedIds.CacheMachineConfiguration,
                "Nix Binary Cache Server",
                "Dedicated NCPS binary cache server with Prometheus node exporter for monitoring.")
            .Tap(cfg => cfg.AddInput(nixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(SeedIds.TimezoneSyncTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixFlakeSettingsTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixBuildOptimisationTemplate, true))
            .Tap(cfg => cfg.AddSystem(SeedIds.CacheMachineSystem, Architecture.X86Linux, "nix-cache"))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.CacheMachineSystem, SeedIds.MinimalBaseTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.CacheMachineSystem, SeedIds.DiskoEfiBtrfsTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.CacheMachineSystem, SeedIds.NcpsCacheServerTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.CacheMachineSystem, SeedIds.PrometheusTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.CacheMachineSystem, SeedIds.AdminUserTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededCacheMachineValues(cfg, options));
    }

    public static Result<Configuration> CreateGnomeWorkstationExample(SeedExampleOptions options)
    {
        const string nixpkgsSource = "github:NixOS/nixpkgs/nixos-unstable";

        return Configuration.Create(
                SeedIds.GnomeWorkstationConfiguration,
                "GNOME Office Workstation",
                "Locked-down GNOME desktop workstation using the local NCPS binary cache and SSH key hardening.")
            .Tap(cfg => cfg.AddInput(nixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(SeedIds.TimezoneSyncTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixFlakeSettingsTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixBuildOptimisationTemplate, true))
            .Tap(cfg => cfg.AddSystem(SeedIds.GnomeWorkstationSystem, Architecture.X86Linux, "gnome-workstation"))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.MinimalBaseTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.DiskoEfiBtrfsTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.GnomeTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.NcpsCacheClientTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.SystemHardeningTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.PrometheusTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.GnomeWorkstationSystem, SeedIds.RegularUserTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededGnomeWorkstationValues(cfg, options));
    }

    public static Result<Configuration> CreateKdeWorkstationExample(SeedExampleOptions options)
    {
        const string nixpkgsSource = "github:NixOS/nixpkgs/nixos-unstable";

        return Configuration.Create(
                SeedIds.KdeWorkstationConfiguration,
                "KDE Office Workstation",
                "Locked-down KDE Plasma workstation using the local NCPS binary cache and SSH key hardening.")
            .Tap(cfg => cfg.AddInput(nixpkgsSource, "nixpkgs"))
            .Tap(cfg => cfg.AddModule(SeedIds.TimezoneSyncTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixFlakeSettingsTemplate, true))
            .Tap(cfg => cfg.AddModule(SeedIds.NixBuildOptimisationTemplate, true))
            .Tap(cfg => cfg.AddSystem(SeedIds.KdeWorkstationSystem, Architecture.X86Linux, "kde-workstation"))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.MinimalBaseTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.DiskoEfiBtrfsTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.KdeTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.NcpsCacheClientTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.SystemHardeningTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.PrometheusTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => cfg.AddSystemModule(SeedIds.KdeWorkstationSystem, SeedIds.RegularUserTemplate,
                [Architecture.X86Linux], true))
            .Tap(cfg => SetSeededKdeWorkstationValues(cfg, options));
    }

    private static void SetSeededPhoeNixDeploymentValues(Configuration cfg, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.TimezoneSyncTemplate);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixFlakeSettingsTemplate);
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

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixBuildOptimisationTemplate);
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

        var system = cfg.SystemSpecifications.Single(s => s.Id == SeedIds.PhoeNixDeploymentSystem);

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.MinimalBaseTemplate);
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

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.DiskoEfiBtrfsTemplate);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var phoenixModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.PhoeNixServiceTemplate);
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

        var adminUserModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.AdminUserTemplate);
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

    private static void SetSeededCacheMachineValues(Configuration cfg, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.TimezoneSyncTemplate);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixFlakeSettingsTemplate);
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

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixBuildOptimisationTemplate);
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

        var system = cfg.SystemSpecifications.Single(s => s.Id == SeedIds.CacheMachineSystem);

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.MinimalBaseTemplate);
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

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.DiskoEfiBtrfsTemplate);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var cacheServerModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.NcpsCacheServerTemplate);
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

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.PrometheusTemplate);
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

        var adminUserModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.AdminUserTemplate);
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

    private static void SetSeededGnomeWorkstationValues(Configuration cfg, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.TimezoneSyncTemplate);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixFlakeSettingsTemplate);
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

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixBuildOptimisationTemplate);
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

        var system = cfg.SystemSpecifications.Single(s => s.Id == SeedIds.GnomeWorkstationSystem);

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.MinimalBaseTemplate);
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

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.DiskoEfiBtrfsTemplate);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var gnomeModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.GnomeTemplate);
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

        var cacheClientModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.NcpsCacheClientTemplate);
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

        var hardeningModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.SystemHardeningTemplate);
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

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.PrometheusTemplate);
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

        var regularUserModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.RegularUserTemplate);
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

    private static void SetSeededKdeWorkstationValues(Configuration cfg, SeedExampleOptions options)
    {
        var timezoneModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.TimezoneSyncTemplate);
        timezoneModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString(options.Timezone),
                SeedPlaceholders.Timezone,
                SeedPlaceholders.Timezone).Value
        ]);

        var nixFlakeModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixFlakeSettingsTemplate);
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

        var nixBuildModule = cfg.Modules.Single(m => m.ModuleTemplateId == SeedIds.NixBuildOptimisationTemplate);
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

        var system = cfg.SystemSpecifications.Single(s => s.Id == SeedIds.KdeWorkstationSystem);

        var baseModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.MinimalBaseTemplate);
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

        var diskoModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.DiskoEfiBtrfsTemplate);
        diskoModule.ReplaceEntries(
        [
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixString("/dev/disk/by-id/REPLACED_AT_RUNTIME"),
                SeedPlaceholders.InstallDisk,
                SeedPlaceholders.InstallDisk).Value
        ]);

        var kdeModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.KdeTemplate);
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

        var cacheClientModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.NcpsCacheClientTemplate);
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

        var hardeningModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.SystemHardeningTemplate);
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

        var prometheusModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.PrometheusTemplate);
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

        var regularUserModule = system.Modules.Single(m => m.ModuleTemplateId == SeedIds.RegularUserTemplate);
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