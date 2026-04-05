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
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixSubstituters,
                SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters).Value,
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                options.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys).Value
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
            TextValue.Create(
                new EntryValueId(Guid.NewGuid()),
                ToNixStringList(options.RootAuthorizedKeys),
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys).Value
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

    private static string ToNixString(string value)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    private static string ToNixStringList(IEnumerable<string> values)
    {
        return $"[ {string.Join(" ", values.Select(ToNixString))} ]";
    }
}