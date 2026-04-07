using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Persistence.Seeding;

internal static class ModuleTemplateSeedFactory
{
    public static Result<IReadOnlyList<ModuleTemplate>> CreateAll()
    {
        var results = new[]
        {
            CreateMinimalBaseTemplate(),
            CreateDiskoTemplate(),
            CreatePrometheusTemplate(),
            CreateTimezoneSyncTemplate(),
            CreateNixFlakeSettingsTemplate(),
            CreateNixBuildOptimisationTemplate()
        };

        var failure = results.FirstOrDefault(r => r.IsFailure);
        if (failure is not null && failure.IsFailure)
            return Result.Failure<IReadOnlyList<ModuleTemplate>>(failure.Error);

        return results.Select(r => r.Value).ToList();
    }

    private static Result<ModuleTemplate> CreateMinimalBaseTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.MinimalBaseTemplate,
                SeedPlaceholders.HostName,
                "\"machineone\"",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text),
            new(
                SeedIds.MinimalBaseTemplate,
                SeedPlaceholders.StateVersion,
                "\"25.11\"",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text),
            new(
                SeedIds.MinimalBaseTemplate,
                SeedPlaceholders.RootAuthorizedKeys,
                "[ \"ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIBogRs9tt7sCKyEM+Vj16pM8tTesXTPWh5nA5lvOc6kM dyrkon603@gmail.com\n\" ]",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text)
        };

        var content =
            $"networking.hostName = {SeedPlaceholders.HostName};\n" +
            "services.openssh.enable = true;\n" +
            $"users.users.root.openssh.authorizedKeys.keys = {SeedPlaceholders.RootAuthorizedKeys};\n" +
            "boot.loader.systemd-boot.enable = true;\n" +
            "boot.loader.efi.canTouchEfiVariables = true;\n" +
            "boot.initrd.availableKernelModules = [ \"virtio_pci\" \"virtio_scsi\" \"virtio_blk\" \"sd_mod\" \"sr_mod\" ];\n" +
            $"system.stateVersion = {SeedPlaceholders.StateVersion};";

        return ModuleTemplate.Create(
                SeedIds.MinimalBaseTemplate,
                "MinimalBaseSystem",
                true,
                ModuleType.System,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("minimal-base-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "minimal-base-test");
                var testContent =
                    "hostNameSet = { expr = HostName != \"\"; expected = true; };\n" +
                    "stateVersionSet = { expr = StateVersion != \"\"; expected = true; };\n" +
                    "keysSet = { expr = RootAuthorizedKeys != []; expected = true; };";
                t.ChangeModuleTest(
                    test.Id,
                    testContent,
                    [SeedPlaceholders.HostName, SeedPlaceholders.StateVersion, SeedPlaceholders.RootAuthorizedKeys]);
            });
    }

    private static Result<ModuleTemplate> CreateDiskoTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.DiskoEfiExt4Template,
                SeedPlaceholders.InstallDisk,
                "\"/dev/sda\"",
                UserInputType.Text,
                EntryBindingKind.RankedDiskCandidate,
                EntryValueKind.Text,
                BindingIndex: 0)
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n" +
            "\n" +
            "disko.devices.disk.main = {\n" +
            "  type = \"disk\";\n" +
            $"  device = {SeedPlaceholders.InstallDisk};\n" +
            "  content = {\n" +
            "    type = \"gpt\";\n" +
            "    partitions = {\n" +
            "      ESP = {\n" +
            "        size = \"512M\";\n" +
            "        type = \"EF00\";\n" +
            "        content = {\n" +
            "          type = \"filesystem\";\n" +
            "          format = \"vfat\";\n" +
            "          mountpoint = \"/boot\";\n" +
            "        };\n" +
            "      };\n" +
            "      root = {\n" +
            "        size = \"100%\";\n" +
            "        content = {\n" +
            "          type = \"filesystem\";\n" +
            "          format = \"ext4\";\n" +
            "          mountpoint = \"/\";\n" +
            "        };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        return ModuleTemplate.Create(
                SeedIds.DiskoEfiExt4Template,
                "DiskoEfiExt4System",
                true,
                ModuleType.System,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("disko-install-disk-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "disko-install-disk-test");
                var testContent =
                    "diskSet = { expr = InstallDisk != \"\"; expected = true; };";
                t.ChangeModuleTest(test.Id, testContent, [SeedPlaceholders.InstallDisk]);
            });
    }

    private static Result<ModuleTemplate> CreatePrometheusTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.PrometheusTemplate,
                SeedPlaceholders.MetricsPort,
                "9100",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text),
            new(
                SeedIds.PrometheusTemplate,
                SeedPlaceholders.OpenFirewall,
                "true",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text)
        };

        var content =
            "systemd.tmpfiles.rules = [\n" +
            "  \"d /var/lib/phoenix 0755 root root -\"\n" +
            "  \"d /var/lib/phoenix/prometheus-textfiles 0755 root root -\"\n" +
            "];\n" +
            "\n" +
            "services.prometheus.exporters.node = {\n" +
            "  enable = true;\n" +
            $"  port = {SeedPlaceholders.MetricsPort};\n" +
            $"  openFirewall = {SeedPlaceholders.OpenFirewall};\n" +
            "  enabledCollectors = [ \"systemd\" \"textfile\" ];\n" +
            "  extraFlags = [\n" +
            "    \"--collector.textfile.directory=/var/lib/phoenix/prometheus-textfiles\"\n" +
            "  ];\n" +
            "};";

        return ModuleTemplate.Create(
                SeedIds.PrometheusTemplate,
                "PrometheusNodeExporter",
                true,
                ModuleType.System,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("prometheus-port-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "prometheus-port-test");
                var testContent =
                    "portRange = { expr = MetricsPort >= 1 && MetricsPort <= 65535; expected = true; };\n" +
                    "firewallValue = { expr = OpenFirewall == true || OpenFirewall == false; expected = true; };";
                t.ChangeModuleTest(
                    test.Id,
                    testContent,
                    [SeedPlaceholders.MetricsPort, SeedPlaceholders.OpenFirewall]);
            });
    }

    private static Result<ModuleTemplate> CreateTimezoneSyncTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.TimezoneSyncTemplate,
                SeedPlaceholders.Timezone,
                "\"Europe/Prague\"",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text)
        };

        var content =
            $"time.timeZone = {SeedPlaceholders.Timezone};\n" +
            "services.timesyncd.enable = true;";

        return ModuleTemplate.Create(
                SeedIds.TimezoneSyncTemplate,
                "TimezoneSync",
                true,
                ModuleType.Generic,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("timezone-sync-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "timezone-sync-test");
                var testContent =
                    "timezoneSet = { expr = Timezone != \"\"; expected = true; };";
                t.ChangeModuleTest(test.Id, testContent, [SeedPlaceholders.Timezone]);
            });
    }

    private static Result<ModuleTemplate> CreateNixFlakeSettingsTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.NixFlakeSettingsTemplate,
                SeedPlaceholders.NixTrustedSubstituters,
                "[ \"https://cache.nixos.org\" \"https://nix-community.cachix.org\" ]",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text),
            new(
                SeedIds.NixFlakeSettingsTemplate,
                SeedPlaceholders.NixTrustedPublicKeys,
                "[ \"cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY=\" \"nix-community.cachix.org-1:mB9FSh9qf2dCimDSUo8Zy7bkq5CX+/rkCWyvRCYg3Fs=\" ]",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text)
        };

        var content =
            "nix.settings = {\n" +
            "  experimental-features = [ \"nix-command\" \"flakes\" ];\n" +
            $"  substituters = {SeedPlaceholders.NixTrustedSubstituters};\n" +
            $"  trusted-public-keys = {SeedPlaceholders.NixTrustedPublicKeys};\n" +
            "};";

        return ModuleTemplate.Create(
                SeedIds.NixFlakeSettingsTemplate,
                "NixFlakeSettings",
                true,
                ModuleType.Generic,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("nix-flake-settings-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "nix-flake-settings-test");
                var testContent =
                    "substitutersSet = { expr = NixTrustedSubstituters != []; expected = true; };\n" +
                    "publicKeysSet = { expr = NixTrustedPublicKeys != []; expected = true; };";
                t.ChangeModuleTest(
                    test.Id,
                    testContent,
                    [SeedPlaceholders.NixTrustedSubstituters, SeedPlaceholders.NixTrustedPublicKeys]);
            });
    }

    private static Result<ModuleTemplate> CreateNixBuildOptimisationTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.NixBuildOptimisationTemplate,
                SeedPlaceholders.NixMaxJobs,
                "\"auto\"",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text),
            new(
                SeedIds.NixBuildOptimisationTemplate,
                SeedPlaceholders.NixCores,
                "1",
                UserInputType.Text,
                EntryBindingKind.UserProvided,
                EntryValueKind.Text)
        };

        var content =
            "nix = {\n" +
            "  settings = {\n" +
            $"    max-jobs = {SeedPlaceholders.NixMaxJobs};\n" +
            $"    cores = {SeedPlaceholders.NixCores};\n" +
            "    auto-optimise-store = true;\n" +
            "  };\n" +
            "  gc = {\n" +
            "    automatic = true;\n" +
            "    dates = \"weekly\";\n" +
            "    options = \"--delete-older-than 7d\";\n" +
            "  };\n" +
            "};";

        return ModuleTemplate.Create(
                SeedIds.NixBuildOptimisationTemplate,
                "NixBuildOptimisation",
                true,
                ModuleType.Generic,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("nix-build-optimisation-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "nix-build-optimisation-test");
                var testContent =
                    "maxJobsPositive = { expr = NixMaxJobs > 0; expected = true; };\n" +
                    "coresPositive = { expr = NixCores > 0; expected = true; };";
                t.ChangeModuleTest(
                    test.Id,
                    testContent,
                    [SeedPlaceholders.NixMaxJobs, SeedPlaceholders.NixCores]);
            });
    }
}