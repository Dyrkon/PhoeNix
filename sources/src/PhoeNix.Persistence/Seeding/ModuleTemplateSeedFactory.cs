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
            CreateCallbackTemplate(),
            CreatePrometheusTemplate()
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
                SeedPlaceholders.HostName,
                UserInputType.Text,
                EntryBindingKind.UserProvided),
            new(
                SeedIds.MinimalBaseTemplate,
                SeedPlaceholders.StateVersion,
                SeedPlaceholders.StateVersion,
                UserInputType.Text,
                EntryBindingKind.UserProvided),
            new(
                SeedIds.MinimalBaseTemplate,
                SeedPlaceholders.RootAuthorizedKeys,
                SeedPlaceholders.RootAuthorizedKeys,
                UserInputType.Text,
                EntryBindingKind.UserProvided)
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
                SeedPlaceholders.InstallDisk,
                UserInputType.Text,
                EntryBindingKind.RankedDiskCandidate,
                0)
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

    private static Result<ModuleTemplate> CreateCallbackTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.CallbackTemplate,
                SeedPlaceholders.CallbackUrl,
                SeedPlaceholders.CallbackUrl,
                UserInputType.Text,
                EntryBindingKind.UserProvided),
            new(
                SeedIds.CallbackTemplate,
                SeedPlaceholders.CallbackToken,
                SeedPlaceholders.CallbackToken,
                UserInputType.Text,
                EntryBindingKind.UserProvided)
        };

        var content =
            "systemd.tmpfiles.rules = [\n" +
            "  \"d /var/lib/phoenix 0755 root root -\"\n" +
            "  \"d /var/lib/phoenix/callback 0755 root root -\"\n" +
            "];\n" +
            "\n" +
            "systemd.services.phoenix-ready-callback = {\n" +
            "  description = \"Notify PhoeNix that machine is ready\";\n" +
            "  wantedBy = [ \"multi-user.target\" ];\n" +
            "  after = [ \"network-online.target\" ];\n" +
            "  wants = [ \"network-online.target\" ];\n" +
            "  serviceConfig = {\n" +
            "    Type = \"oneshot\";\n" +
            "    ExecStart = let\n" +
            "      callbackScript = pkgs.writeShellScript \"phoenix-ready-callback\" ''\n" +
            "        set -euo pipefail\n" +
            $"        {pkgsCurl()} --fail --silent --show-error \\\n" +
            "          -X POST \\\n" +
            $"          -H \"Authorization: Bearer ${{{SeedPlaceholders.CallbackToken}}}\" \\\n" +
            $"          ${{{SeedPlaceholders.CallbackUrl}}}\n" +
            "      '';\n" +
            "    in callbackScript;\n" +
            "  };\n" +
            "};";

        return ModuleTemplate.Create(
                SeedIds.CallbackTemplate,
                "PhoenixReadyCallback",
                true,
                ModuleType.System,
                [Architecture.X86Linux, Architecture.Aarch64Linux])
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest("callback-values-test"))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == "callback-values-test");
                var testContent =
                    "callbackUrlSet = { expr = CallbackUrl != \"\"; expected = true; };\n" +
                    "callbackTokenSet = { expr = CallbackToken != \"\"; expected = true; };";
                t.ChangeModuleTest(
                    test.Id,
                    testContent,
                    [SeedPlaceholders.CallbackUrl, SeedPlaceholders.CallbackToken]);
            });
    }

    private static Result<ModuleTemplate> CreatePrometheusTemplate()
    {
        var definitions = new List<EntryValueDefinition>
        {
            new(
                SeedIds.PrometheusTemplate,
                SeedPlaceholders.MetricsPort,
                SeedPlaceholders.MetricsPort,
                UserInputType.Text,
                EntryBindingKind.UserProvided),
            new(
                SeedIds.PrometheusTemplate,
                SeedPlaceholders.OpenFirewall,
                SeedPlaceholders.OpenFirewall,
                UserInputType.Text,
                EntryBindingKind.UserProvided)
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

    private static string pkgsCurl()
    {
        return "${pkgs.curl}/bin/curl";
    }
}