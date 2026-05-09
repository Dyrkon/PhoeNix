using System.Text.Json;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Persistence.Seeding;

internal static class ModuleTemplateSeedFactory
{
    public static Result<(IReadOnlyList<ModuleTemplate> Templates, IReadOnlyDictionary<string, ModuleTemplateId> ById)>
        CreateAll(UserId ownerId)
    {
        var results = new[]
        {
            CreateMinimalBaseTemplate(ownerId),
            CreateDiskoTemplate(ownerId),
            CreatePrometheusTemplate(ownerId),
            CreateTimezoneSyncTemplate(ownerId),
            CreateNixFlakeSettingsTemplate(ownerId),
            CreateNixBuildOptimisationTemplate(ownerId),
            CreatePhoeNixServiceTemplate(ownerId),
            CreateNcpsCacheServerTemplate(ownerId),
            CreateNcpsCacheClientTemplate(ownerId),
            CreateKdeWorkstationTemplate(ownerId),
            CreateGnomeWorkstationTemplate(ownerId),
            CreateSystemHardeningTemplate(ownerId),
            CreateItSupportTemplate(ownerId),
            CreateAmdGpuTemplate(ownerId),
            CreateNvidiaGpuTemplate(ownerId),
            CreateDiskoEfiBtrfsTemplate(ownerId),
            CreateDiskoEfiLuksExt4Template(ownerId),
            CreateDiskoEfiZfsTemplate(ownerId),
            CreateDiskoSsdHddTemplate(ownerId),
            CreateAdminUserTemplate(ownerId),
            CreateRegularUserTemplate(ownerId)
        };

        var failure = results.FirstOrDefault(r => r.IsFailure);
        if (failure is not null && failure.IsFailure)
            return Result.Failure<(IReadOnlyList<ModuleTemplate>, IReadOnlyDictionary<string, ModuleTemplateId>)>(
                failure.Error);

        var templates = results.Select(r => r.Value).ToList();
        var byId = (IReadOnlyDictionary<string, ModuleTemplateId>)templates.ToDictionary(t => t.Name, t => t.Id);
        return Result
            .Success<(IReadOnlyList<ModuleTemplate> Templates, IReadOnlyDictionary<string, ModuleTemplateId> ById)>((
                templates, byId));
    }

    private static Result<ModuleTemplate> BuildTemplate(
        UserId ownerId,
        ModuleTemplateId templateId,
        string name,
        ModuleType moduleType,
        string content,
        List<EntryValueDefinition> definitions,
        string testName,
        string testContent,
        List<string> testPlaceholders,
        Architecture[]? architectures = null)
    {
        architectures ??= [Architecture.X86Linux, Architecture.Aarch64Linux];

        return ModuleTemplate.Create(templateId, ownerId, name, true, moduleType, architectures)
            .Tap(t => t.ChangeContent(content, definitions))
            .Tap(t => t.AddModuleTest(testName))
            .Tap(t =>
            {
                var test = t.Tests.Single(x => x.Name == testName);
                t.ChangeModuleTest(test.Id, testContent, testPlaceholders);
            });
    }

    private static Result<ModuleTemplate> CreateMinimalBaseTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.HostName, SeedPlaceholders.HostName,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"machineone\""),
            new(templateId, SeedPlaceholders.StateVersion, SeedPlaceholders.StateVersion,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"25.11\""),
            new(templateId, SeedPlaceholders.RootAuthorizedKeys, SeedPlaceholders.RootAuthorizedKeys,
                EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>
                {
                    "\"YOUR SSH KEY\""
                }))
        };

        var content =
            $"networking.hostName = {SeedPlaceholders.HostName};\n" +
            "services.openssh.enable = true;\n" +
            $"users.users.root.openssh.authorizedKeys.keys = {SeedPlaceholders.RootAuthorizedKeys};\n" +
            "boot.loader.systemd-boot.enable = true;\n" +
            "boot.loader.efi.canTouchEfiVariables = true;\n" +
            "boot.initrd.availableKernelModules = [ \"virtio_pci\" \"virtio_scsi\" \"virtio_blk\" \"sd_mod\" \"sr_mod\" ];\n" +
            $"system.stateVersion = {SeedPlaceholders.StateVersion};";

        var testContent =
            "hostNameSet = { expr = HostName != \"\"; expected = true; };\n" +
            "stateVersionSet = { expr = StateVersion != \"\"; expected = true; };\n" +
            "keysSet = { expr = RootAuthorizedKeys != []; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "MinimalBaseSystem",
            ModuleType.System,
            content,
            definitions,
            "minimal-base-test",
            testContent,
            [SeedPlaceholders.HostName, SeedPlaceholders.StateVersion, SeedPlaceholders.RootAuthorizedKeys]
        );
    }

    private static Result<ModuleTemplate> CreateDiskoTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.InstallDisk, SeedPlaceholders.InstallDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sda\"", BindingIndex: 0)
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
            "disko.devices.disk.main = {\n" +
            "  type = \"disk\";\n" +
            $"  device = {SeedPlaceholders.InstallDisk};\n" +
            "  content = {\n" +
            "    type = \"gpt\";\n" +
            "    partitions = {\n" +
            "      ESP = { size = \"512M\"; type = \"EF00\"; content = { type = \"filesystem\"; format = \"vfat\"; mountpoint = \"/boot\"; }; };\n" +
            "      root = { size = \"100%\"; content = { type = \"filesystem\"; format = \"ext4\"; mountpoint = \"/\"; }; };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        var testContent = "diskSet = { expr = InstallDisk != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "DiskoEfiExt4System",
            ModuleType.System,
            content,
            definitions,
            "disko-install-disk-test",
            testContent,
            [SeedPlaceholders.InstallDisk]
        );
    }

    private static Result<ModuleTemplate> CreatePrometheusTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.MetricsPort, SeedPlaceholders.MetricsPort,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "9100"),
            new(templateId, SeedPlaceholders.OpenFirewall, SeedPlaceholders.OpenFirewall,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "true",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "true", "false" }))
        };

        var content =
            "systemd.tmpfiles.rules = [\n  \"d /var/lib/phoenix 0755 root root -\"\n  \"d /var/lib/phoenix/prometheus-textfiles 0755 root root -\"\n];\n\n" +
            "services.prometheus.exporters.node = {\n" +
            "  enable = true;\n" +
            $"  port = {SeedPlaceholders.MetricsPort};\n" +
            $"  openFirewall = {SeedPlaceholders.OpenFirewall};\n" +
            "  enabledCollectors = [ \"systemd\" \"textfile\" ];\n" +
            "  extraFlags = [ \"--collector.textfile.directory=/var/lib/phoenix/prometheus-textfiles\" ];\n" +
            "};";

        var testContent =
            "portRange = { expr = MetricsPort >= 1 && MetricsPort <= 65535; expected = true; };\n" +
            "firewallValue = { expr = OpenFirewall == true || OpenFirewall == false; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "PrometheusNodeExporter",
            ModuleType.System,
            content,
            definitions,
            "prometheus-port-test",
            testContent,
            [SeedPlaceholders.MetricsPort, SeedPlaceholders.OpenFirewall]
        );
    }

    private static Result<ModuleTemplate> CreateTimezoneSyncTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.Timezone, SeedPlaceholders.Timezone,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"Europe/Prague\"")
        };

        var content = $"time.timeZone = {SeedPlaceholders.Timezone};\nservices.timesyncd.enable = true;";
        var testContent = "timezoneSet = { expr = Timezone != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "TimezoneSync",
            ModuleType.Generic,
            content,
            definitions,
            "timezone-sync-test",
            testContent,
            [SeedPlaceholders.Timezone]
        );
    }

    private static Result<ModuleTemplate> CreateNixFlakeSettingsTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.NixTrustedSubstituters,
                SeedPlaceholders.NixTrustedSubstituters, EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>
                    { "\"https://cache.nixos.org\"", "\"https://nix-community.cachix.org\"" })),
            new(templateId, SeedPlaceholders.NixTrustedPublicKeys,
                SeedPlaceholders.NixTrustedPublicKeys, EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>
                {
                    "\"cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY=\"",
                    "\"nix-community.cachix.org-1:mB9FSh9qf2dCimDSUo8Zy7bkq5CX+/rkCWyvRCYg3Fs=\""
                }))
        };

        var content =
            "nix.settings = {\n" +
            "  experimental-features = [ \"nix-command\" \"flakes\" ];\n" +
            $"  substituters = {SeedPlaceholders.NixTrustedSubstituters};\n" +
            $"  trusted-public-keys = {SeedPlaceholders.NixTrustedPublicKeys};\n" +
            "};";

        var testContent =
            "substitutersSet = { expr = NixTrustedSubstituters != []; expected = true; };\n" +
            "publicKeysSet = { expr = NixTrustedPublicKeys != []; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "NixFlakeSettings",
            ModuleType.Generic,
            content,
            definitions,
            "nix-flake-settings-test",
            testContent,
            [SeedPlaceholders.NixTrustedSubstituters, SeedPlaceholders.NixTrustedPublicKeys]
        );
    }

    private static Result<ModuleTemplate> CreateNixBuildOptimisationTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.NixMaxJobs, SeedPlaceholders.NixMaxJobs,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"auto\""),
            new(templateId, SeedPlaceholders.NixCores, SeedPlaceholders.NixCores,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "1")
        };

        var content =
            "nix = {\n  settings = {\n" +
            $"    max-jobs = {SeedPlaceholders.NixMaxJobs};\n" +
            $"    cores = {SeedPlaceholders.NixCores};\n" +
            "    auto-optimise-store = true;\n  };\n" +
            "  gc = {\n    automatic = true;\n    dates = \"weekly\";\n    options = \"--delete-older-than 7d\";\n  };\n};";

        var testContent =
            "maxJobsPositive = { expr = NixMaxJobs > 0; expected = true; };\n" +
            "coresPositive = { expr = NixCores > 0; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "NixBuildOptimisation",
            ModuleType.Generic,
            content,
            definitions,
            "nix-build-optimisation-test",
            testContent,
            [SeedPlaceholders.NixMaxJobs, SeedPlaceholders.NixCores]
        );
    }

    private static Result<ModuleTemplate> CreatePhoeNixServiceTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.PhoenixPublicBaseUrl,
                SeedPlaceholders.PhoenixPublicBaseUrl, EntryBindingKind.UserProvided, EntryValueKind.Text,
                "\"http://REPLACE-HOSTNAME\"")
        };

        var content =
            "imports = [ inputs.phoenix.nixosModules.default ];\n\n" +
            "services.phoenix = {\n" +
            "  enable = true;\n" +
            "  api.environment = {\n" +
            "    \"Logging__LogLevel__Default\" = \"Information\";\n" +
            "    \"Logging__LogLevel__Microsoft.AspNetCore\" = \"Warning\";\n" +
            "    \"NetbootHost__HostExecutablePath\" = \"/run/wrappers/bin/pixiecore\";\n" +
            "    \"SeedExample__HostName\" = \"phoenix-demo\";\n" +
            "    \"SeedExample__StateVersion\" = \"25.11\";\n" +
            "    \"SeedExample__RootAuthorizedKeys__0\" = \"YOUR SSH KEY\";\n" +
            $"    \"SeedExample__PublicBaseUrl\" = {SeedPlaceholders.PhoenixPublicBaseUrl};\n" +
            "    \"SeedExample__MetricsPort\" = \"9100\";\n" +
            "    \"SeedExample__OpenFirewall\" = \"true\";\n" +
            "  };\n" +
            "  monitoring = {\n" +
            "    enable = true;\n" +
            "    prometheusServer = {\n" +
            "      enable = true;\n" +
            "      ui = { public = true; nginxProxy = true; };\n" +
            "    };\n" +
            "    nodeExporter.enable = true;\n" +
            "  };\n" +
            "  nginx.enable = true;\n" +
            "  mcpServer.enable = true;\n" +
            "};";

        var testContent = "publicUrlSet = { expr = PhoenixPublicBaseUrl != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "PhoeNixService",
            ModuleType.System,
            content,
            definitions,
            "phoenix-service-test",
            testContent,
            [SeedPlaceholders.PhoenixPublicBaseUrl]
        ).Tap(t => t.SetRequiredInputs([("phoenix", "git+ssh://git@github.com/Dyrkon/PhoeNix")]));
    }

    private static Result<ModuleTemplate> CreateNcpsCacheServerTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.NcpsCacheHostName,
                SeedPlaceholders.NcpsCacheHostName, EntryBindingKind.UserProvided, EntryValueKind.Text,
                "\"machine-hostname.lan\""),
            new(templateId, SeedPlaceholders.NcpsServerAddress,
                SeedPlaceholders.NcpsServerAddress, EntryBindingKind.UserProvided, EntryValueKind.Text,
                "\":8501\"")
        };

        var content =
            "services.ncps = {\n" +
            "  enable = true;\n" +
            $"  cache.hostName = {SeedPlaceholders.NcpsCacheHostName};\n" +
            $"  server.addr = {SeedPlaceholders.NcpsServerAddress};\n" +
            "  upstream = {\n" +
            "    caches = [ \"https://cache.nixos.org\" ];\n" +
            "    publicKeys = [ \"cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY=\" ];\n" +
            "  };\n" +
            "};\n" +
            "networking.firewall.allowedTCPPorts = [ 8501 ];";

        var testContent = "hostNameSet = { expr = NcpsCacheHostName != \"\"; expected = true; };\n" +
                          "serverAddrSet = { expr = NcpsServerAddress != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "NcpsCacheServer",
            ModuleType.Generic,
            content,
            definitions,
            "ncps-cache-server-test",
            testContent,
            [SeedPlaceholders.NcpsCacheHostName, SeedPlaceholders.NcpsServerAddress]
        );
    }

    private static Result<ModuleTemplate> CreateNcpsCacheClientTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.LocalCacheSubstituters,
                SeedPlaceholders.LocalCacheSubstituters, EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string> { "\"http://machine-hostname.lan:8501\"" })),
            new(templateId, SeedPlaceholders.LocalCachePublicKeys,
                SeedPlaceholders.LocalCachePublicKeys, EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>
                    { "\"cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY=\"" }))
        };

        var content =
            "nix.settings = {\n" +
            $"  substituters = {SeedPlaceholders.LocalCacheSubstituters};\n" +
            $"  trusted-public-keys = {SeedPlaceholders.LocalCachePublicKeys};\n" +
            "};";

        var testContent = "substitutersSet = { expr = LocalSubstituters != []; expected = true; };\n" +
                          "publicKeysSet = { expr = LocalTrustedPublicKeys != []; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "NcpsCacheClient",
            ModuleType.Generic,
            content,
            definitions,
            "ncps-cache-client-test",
            testContent,
            [SeedPlaceholders.LocalCacheSubstituters, SeedPlaceholders.LocalCachePublicKeys]
        );
    }

    private static Result<ModuleTemplate> CreateKdeWorkstationTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.Locale, SeedPlaceholders.Locale,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"en_US.UTF-8\""),
            new(templateId, SeedPlaceholders.KeyboardLayout, SeedPlaceholders.KeyboardLayout,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"us\""),
            new(templateId, SeedPlaceholders.KdePrinting, SeedPlaceholders.KdePrinting,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" })),
            new(templateId, SeedPlaceholders.KdeConnect, SeedPlaceholders.KdeConnect,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" }))
        };

        var content =
            $"i18n.defaultLocale = {SeedPlaceholders.Locale};\n" +
            $"services.xserver.xkb.layout = {SeedPlaceholders.KeyboardLayout};\n" +
            "services.desktopManager.plasma6.enable = true;\n" +
            "services.displayManager.sddm.enable = true;\n" +
            "services.displayManager.sddm.wayland.enable = true;\n" +
            "environment.systemPackages = with pkgs; [\n" +
            "  firefox\n" +
            "  libreoffice-qt\n" +
            "  kdePackages.okular\n" +
            "  kdePackages.gwenview\n" +
            "];\n" +
            $"services.printing.enable = {SeedPlaceholders.KdePrinting};\n" +
            $"programs.kdeconnect.enable = {SeedPlaceholders.KdeConnect};";

        var testContent = "localeSet = { expr = Locale != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "KdeWorkstation",
            ModuleType.System,
            content,
            definitions,
            "kde-workstation-test",
            testContent,
            [
                SeedPlaceholders.Locale, SeedPlaceholders.KeyboardLayout, SeedPlaceholders.KdePrinting,
                SeedPlaceholders.KdeConnect
            ]
        );
    }

    private static Result<ModuleTemplate> CreateGnomeWorkstationTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.Locale, SeedPlaceholders.Locale,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"en_US.UTF-8\""),
            new(templateId, SeedPlaceholders.KeyboardLayout, SeedPlaceholders.KeyboardLayout,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"us\""),
            new(templateId, SeedPlaceholders.GnomeCoreApps, SeedPlaceholders.GnomeCoreApps,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" })),
            new(templateId, SeedPlaceholders.GnomeGames, SeedPlaceholders.GnomeGames,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" })),
            new(templateId, SeedPlaceholders.GnomeDeveloperTools, SeedPlaceholders.GnomeDeveloperTools,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" }))
        };

        var content =
            $"i18n.defaultLocale = {SeedPlaceholders.Locale};\n" +
            $"services.xserver.xkb.layout = {SeedPlaceholders.KeyboardLayout};\n" +
            "services.displayManager.gdm.enable = true;\n" +
            "services.desktopManager.gnome.enable = true;\n" +
            $"services.gnome.core-apps.enable = {SeedPlaceholders.GnomeCoreApps};\n" +
            $"services.gnome.games.enable = {SeedPlaceholders.GnomeGames};\n" +
            $"services.gnome.core-developer-tools.enable = {SeedPlaceholders.GnomeDeveloperTools};\n" +
            "environment.gnome.excludePackages = with pkgs; [ gnome-tour gnome-user-docs ];\n" +
            "environment.systemPackages = with pkgs; [\n" +
            "  firefox\n" +
            "  libreoffice-fresh\n" +
            "  evince\n" +
            "  eog\n" +
            "];";

        var testContent = "localeSet = { expr = Locale != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "GnomeWorkstation",
            ModuleType.System,
            content,
            definitions,
            "gnome-workstation-test",
            testContent,
            [
                SeedPlaceholders.Locale, SeedPlaceholders.KeyboardLayout, SeedPlaceholders.GnomeCoreApps,
                SeedPlaceholders.GnomeGames, SeedPlaceholders.GnomeDeveloperTools
            ]
        );
    }

    private static Result<ModuleTemplate> CreateSystemHardeningTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.Sandbox, SeedPlaceholders.Sandbox,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "true",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "true", "false" })),
            new(templateId, SeedPlaceholders.AdminUser, SeedPlaceholders.AdminUser,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"admin\""),
            new(templateId, SeedPlaceholders.SshPermitRootLogin,
                SeedPlaceholders.SshPermitRootLogin, EntryBindingKind.UserProvided, EntryValueKind.SingleChoice,
                "\"prohibit-password\"",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "\"prohibit-password\"", "\"no\"" }))
        };

        var content =
            "users.mutableUsers = false;\n" +
            $"nix.settings.trusted-users = [ \"root\" {SeedPlaceholders.AdminUser} ];\n" +
            $"nix.settings.allowed-users = [ \"root\" {SeedPlaceholders.AdminUser} ];\n" +
            "security.sudo.wheelNeedsPassword = true;\n" +
            "security.auditd.enable = true;\n " +
            "security.audit.enable = true;\n  " +
            "security.audit.rules = [\n" +
            "    \"-a exit,always -F arch=b64 -S execve\"\n  ];\n" +
            $"nix.useSandbox = {SeedPlaceholders.Sandbox};\n" +
            "environment.defaultPackages = lib.mkForce [];\n" +
            "services.openssh.settings = {\n" +
            "  PasswordAuthentication = false;\n" +
            "  KbdInteractiveAuthentication = false;\n" +
            $"  PermitRootLogin = {SeedPlaceholders.SshPermitRootLogin};\n" +
            "};";

        var testContent =
            "adminUserSet = { expr = AdminUser != \"\"; expected = true; };\n" +
            "sshPermitRootLoginSet = { expr = SshPermitRootLogin != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "SystemHardening",
            ModuleType.Generic,
            content,
            definitions,
            "system-hardening-test",
            testContent,
            [SeedPlaceholders.AdminUser, SeedPlaceholders.SshPermitRootLogin]
        );
    }

    private static Result<ModuleTemplate> CreateDiskoEfiBtrfsTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.InstallDisk, SeedPlaceholders.InstallDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sda\"", BindingIndex: 0)
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
            "disko.devices.disk.main = {\n" +
            "  type = \"disk\";\n" +
            $"  device = {SeedPlaceholders.InstallDisk};\n" +
            "  content = {\n" +
            "    type = \"gpt\";\n" +
            "    partitions = {\n" +
            "      ESP = { size = \"512M\"; type = \"EF00\"; content = { type = \"filesystem\"; format = \"vfat\"; mountpoint = \"/boot\"; }; };\n" +
            "      root = {\n" +
            "        size = \"100%\";\n" +
            "        content = {\n" +
            "          type = \"btrfs\";\n" +
            "          extraArgs = [ \"-L\" \"nixos\" \"-f\" ];\n" +
            "          subvolumes = {\n" +
            "            \"/root\" = { mountpoint = \"/\"; mountOptions = [ \"compress=zstd\" \"noatime\" ]; };\n" +
            "            \"/home\" = { mountpoint = \"/home\"; mountOptions = [ \"compress=zstd\" \"noatime\" ]; };\n" +
            "            \"/nix\"  = { mountpoint = \"/nix\"; mountOptions = [ \"compress=zstd\" \"noatime\" ]; };\n" +
            "          };\n" +
            "        };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        var testContent = "diskSet = { expr = InstallDisk != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "DiskoEfiBtrfs",
            ModuleType.System,
            content,
            definitions,
            "disko-btrfs-disk-test",
            testContent,
            [SeedPlaceholders.InstallDisk]
        );
    }

    private static Result<ModuleTemplate> CreateDiskoEfiLuksExt4Template(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.InstallDisk, SeedPlaceholders.InstallDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sda\"", BindingIndex: 0)
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
            "disko.devices.disk.main = {\n" +
            "  type = \"disk\";\n" +
            $"  device = {SeedPlaceholders.InstallDisk};\n" +
            "  content = {\n" +
            "    type = \"gpt\";\n" +
            "    partitions = {\n" +
            "      ESP = { size = \"512M\"; type = \"EF00\"; content = { type = \"filesystem\"; format = \"vfat\"; mountpoint = \"/boot\"; }; };\n" +
            "      luks = {\n" +
            "        size = \"100%\";\n" +
            "        content = {\n" +
            "          type = \"luks\";\n" +
            "          name = \"cryptroot\";\n" +
            "          extraOpenArgs = [ \"--allow-discards\" ];\n" +
            "          content = {\n" +
            "            type = \"filesystem\";\n" +
            "            format = \"ext4\";\n" +
            "            mountpoint = \"/\";\n" +
            "          };\n" +
            "        };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        var testContent = "diskSet = { expr = InstallDisk != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "DiskoEfiLuksExt4",
            ModuleType.System,
            content,
            definitions,
            "disko-luks-disk-test",
            testContent,
            [SeedPlaceholders.InstallDisk]
        );
    }

    private static Result<ModuleTemplate> CreateDiskoEfiZfsTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.InstallDisk, SeedPlaceholders.InstallDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sda\"", BindingIndex: 0),
            new(templateId, SeedPlaceholders.ZfsHostId, SeedPlaceholders.ZfsHostId,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"deadbeef\"")
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
            $"networking.hostId = {SeedPlaceholders.ZfsHostId};\n\n" +
            "disko.devices = {\n" +
            "  disk.main = {\n" +
            "    type = \"disk\";\n" +
            $"    device = {SeedPlaceholders.InstallDisk};\n" +
            "    content = {\n" +
            "      type = \"gpt\";\n" +
            "      partitions = {\n" +
            "        ESP = { size = \"512M\"; type = \"EF00\"; content = { type = \"filesystem\"; format = \"vfat\"; mountpoint = \"/boot\"; }; };\n" +
            "        zfs = { size = \"100%\"; content = { type = \"zfs\"; pool = \"zroot\"; }; };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "  zpool.zroot = {\n" +
            "    type = \"zpool\";\n" +
            "    rootFsOptions = { compression = \"lz4\"; \"com.sun:auto-snapshot\" = \"false\"; };\n" +
            "    datasets = {\n" +
            "      \"root\" = { type = \"zfs_fs\"; mountpoint = \"/\"; options.mountpoint = \"legacy\"; };\n" +
            "      \"home\" = { type = \"zfs_fs\"; mountpoint = \"/home\"; options.mountpoint = \"legacy\"; };\n" +
            "      \"nix\"  = { type = \"zfs_fs\"; mountpoint = \"/nix\"; options.mountpoint = \"legacy\"; options.\"com.sun:auto-snapshot\" = \"false\"; };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        var testContent =
            "diskSet = { expr = InstallDisk != \"\"; expected = true; };\n" +
            "hostIdSet = { expr = ZfsHostId != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "DiskoEfiZfs",
            ModuleType.System,
            content,
            definitions,
            "disko-zfs-disk-test",
            testContent,
            [SeedPlaceholders.InstallDisk, SeedPlaceholders.ZfsHostId]
        );
    }

    private static Result<ModuleTemplate> CreateDiskoSsdHddTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.SsdDisk, SeedPlaceholders.SsdDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sda\"", BindingIndex: 0),
            new(templateId, SeedPlaceholders.HddDisk, SeedPlaceholders.HddDisk,
                EntryBindingKind.RankedDiskCandidate, EntryValueKind.Text, "\"/dev/sdb\"", BindingIndex: 1)
        };

        var content =
            "imports = [ inputs.disko.nixosModules.disko ];\n\n" +
            "disko.devices.disk = {\n" +
            "  ssd = {\n" +
            "    type = \"disk\";\n" +
            $"    device = {SeedPlaceholders.SsdDisk};\n" +
            "    content = {\n" +
            "      type = \"gpt\";\n" +
            "      partitions = {\n" +
            "        ESP = { size = \"512M\"; type = \"EF00\"; content = { type = \"filesystem\"; format = \"vfat\"; mountpoint = \"/boot\"; }; };\n" +
            "        root = { size = \"100%\"; content = { type = \"filesystem\"; format = \"ext4\"; mountpoint = \"/\"; }; };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "  hdd = {\n" +
            "    type = \"disk\";\n" +
            $"    device = {SeedPlaceholders.HddDisk};\n" +
            "    content = {\n" +
            "      type = \"gpt\";\n" +
            "      partitions = {\n" +
            "        data = { size = \"100%\"; content = { type = \"filesystem\"; format = \"ext4\"; mountpoint = \"/home\"; }; };\n" +
            "      };\n" +
            "    };\n" +
            "  };\n" +
            "};";

        var testContent =
            "ssdSet = { expr = SsdDisk != \"\"; expected = true; };\n" +
            "hddSet = { expr = HddDisk != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "DiskoSsdHdd",
            ModuleType.System,
            content,
            definitions,
            "disko-ssd-hdd-test",
            testContent,
            [SeedPlaceholders.SsdDisk, SeedPlaceholders.HddDisk]
        );
    }

    private static Result<ModuleTemplate> CreateAmdGpuTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.GpuEnable32Bit, SeedPlaceholders.GpuEnable32Bit,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "true",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "true", "false" }))
        };

        var content =
            "boot.initrd.kernelModules = [ \"amdgpu\" ];\n" +
            "hardware.graphics = {\n" +
            "  enable = true;\n" +
            $"  enable32Bit = {SeedPlaceholders.GpuEnable32Bit};\n" +
            "  extraPackages = with pkgs; [ amdvlk rocmPackages.clr.icd ];\n" +
            "  extraPackages32 = with pkgs; [ driversi686Linux.amdvlk ];\n" +
            "};";

        var testContent =
            "enable32BitValid = { expr = Enable32Bit == true || Enable32Bit == false; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "AmdGpu",
            ModuleType.Generic,
            content,
            definitions,
            "amd-gpu-test",
            testContent,
            [SeedPlaceholders.GpuEnable32Bit]
        );
    }

    private static Result<ModuleTemplate> CreateNvidiaGpuTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.NvidiaOpenKernel, SeedPlaceholders.NvidiaOpenKernel,
                EntryBindingKind.UserProvided, EntryValueKind.SingleChoice, "false",
                OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" })),
            new(templateId, SeedPlaceholders.NvidiaPowerManagement,
                SeedPlaceholders.NvidiaPowerManagement, EntryBindingKind.UserProvided, EntryValueKind.SingleChoice,
                "false", OptionsJson: JsonSerializer.Serialize(new List<string> { "false", "true" })),
            new(templateId, SeedPlaceholders.NvidiaDriverChannel,
                SeedPlaceholders.NvidiaDriverChannel, EntryBindingKind.UserProvided, EntryValueKind.Text, "\"stable\"")
        };

        var content =
            "services.xserver.videoDrivers = [ \"nvidia\" ];\n" +
            "hardware.graphics = {\n" +
            "  enable = true;\n" +
            "  enable32Bit = true;\n" +
            "};\n" +
            "hardware.nvidia = {\n" +
            "  modesetting.enable = true;\n" +
            $"  powerManagement.enable = {SeedPlaceholders.NvidiaPowerManagement};\n" +
            "  powerManagement.finegrained = false;\n" +
            $"  open = {SeedPlaceholders.NvidiaOpenKernel};\n" +
            "  nvidiaSettings = true;\n" +
            $"  package = config.boot.kernelPackages.nvidiaPackages.{SeedPlaceholders.NvidiaDriverChannel};\n" +
            "};";

        var testContent =
            "openValid = { expr = NvidiaOpenKernel == true || NvidiaOpenKernel == false; expected = true; };\n" +
            "powerValid = { expr = NvidiaPowerManagement == true || NvidiaPowerManagement == false; expected = true; };\n" +
            "channelSet = { expr = NvidiaDriverChannel != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "NvidiaGpu",
            ModuleType.Generic,
            content,
            definitions,
            "nvidia-gpu-test",
            testContent,
            [
                SeedPlaceholders.NvidiaOpenKernel, SeedPlaceholders.NvidiaPowerManagement,
                SeedPlaceholders.NvidiaDriverChannel
            ]
        );
    }

    private static Result<ModuleTemplate> CreateItSupportTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.LogRetentionDays, SeedPlaceholders.LogRetentionDays,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"30day\"")
        };

        var content =
            "services.xrdp = {\n" +
            "  enable = true;\n" +
            "  openFirewall = true;\n" +
            "};\n\n" +
            "environment.systemPackages = with pkgs; [ rustdesk ];\n" +
            "systemd.services.rustdesk = {\n" +
            "  description = \"RustDesk Remote Desktop\";\n" +
            "  wantedBy = [ \"multi-user.target\" ];\n" +
            "  serviceConfig = {\n" +
            "    ExecStart = \"${pkgs.rustdesk}/bin/rustdesk --service\";\n" +
            "    Restart = \"on-failure\";\n" +
            "    RestartSec = \"5s\";\n" +
            "  };\n" +
            "};\n\n" +
            "services.journald.extraConfig = ''\n" +
            $"  Storage=persistent\n  MaxRetentionSec={SeedPlaceholders.LogRetentionDays}\n" +
            "'';";

        var testContent = "retentionSet = { expr = LogRetentionDays != \"\"; expected = true; };";

        return BuildTemplate(
            ownerId,
            templateId,
            "ItSupport",
            ModuleType.Generic,
            content,
            definitions,
            "it-support-test",
            testContent,
            [SeedPlaceholders.LogRetentionDays]
        );
    }

    private static Result<ModuleTemplate> CreateAdminUserTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.UserName, SeedPlaceholders.UserName,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"admin\""),
            new(templateId, SeedPlaceholders.UserDescription, SeedPlaceholders.UserDescription,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"System Administrator\""),
            new(templateId, SeedPlaceholders.UserGroups, SeedPlaceholders.UserGroups,
                EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string> { "\"wheel\"" })),
            new(templateId, SeedPlaceholders.UserAuthorizedKeys, SeedPlaceholders.UserAuthorizedKeys,
                EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>())),
            new(templateId, SeedPlaceholders.UserInitialPassword, SeedPlaceholders.UserInitialPassword,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "\"changeme\"")
        };

        var content =
            $"  users.users.\"${{{SeedPlaceholders.UserName}}}\" = {{\n" +
            "    isNormalUser = true;\n" +
            $"    description = {SeedPlaceholders.UserDescription};\n" +
            $"    home = \"/home/${{{SeedPlaceholders.UserName}}}\";\n" +
            "    createHome = true;\n" +
            $"    extraGroups = {SeedPlaceholders.UserGroups};\n" +
            $"    openssh.authorizedKeys.keys = {SeedPlaceholders.UserAuthorizedKeys};\n" +
            $"    initialPassword = {SeedPlaceholders.UserInitialPassword};" +
            $"}};";

        var testContent =
            $"userNameSet = {{ expr = \"{SeedPlaceholders.UserName}\" != \"\"; expected = true; }};";

        return BuildTemplate(
            ownerId,
            templateId,
            "Admin User",
            ModuleType.Generic,
            content,
            definitions,
            "admin-user-test",
            testContent,
            [
                SeedPlaceholders.UserName, SeedPlaceholders.UserDescription, SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserAuthorizedKeys, SeedPlaceholders.UserInitialPassword
            ]
        );
    }

    private static Result<ModuleTemplate> CreateRegularUserTemplate(UserId ownerId)
    {
        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var definitions = new List<EntryValueDefinition>
        {
            new(templateId, SeedPlaceholders.UserName, SeedPlaceholders.UserName,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "user"),
            new(templateId, SeedPlaceholders.UserDescription, SeedPlaceholders.UserDescription,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "Desktop User"),
            new(templateId, SeedPlaceholders.UserGroups, SeedPlaceholders.UserGroups,
                EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string> { "\"video\"", "\"audio\"", "\"networkmanager\"" })),
            new(templateId, SeedPlaceholders.UserAuthorizedKeys, SeedPlaceholders.UserAuthorizedKeys,
                EntryBindingKind.UserProvided, EntryValueKind.List,
                JsonSerializer.Serialize(new List<string>())),
            new(templateId, SeedPlaceholders.UserInitialPassword, SeedPlaceholders.UserInitialPassword,
                EntryBindingKind.UserProvided, EntryValueKind.Text, "changeme")
        };

        var content =
            $"  users.users.\"${{{SeedPlaceholders.UserName}}}\" = {{\n" +
            "    isNormalUser = true;\n" +
            $"    description = {SeedPlaceholders.UserDescription};\n" +
            $"    home = \"/home/${{{SeedPlaceholders.UserName}}}\";\n" +
            "    createHome = true;\n" +
            $"    extraGroups = {SeedPlaceholders.UserGroups};\n" +
            $"    openssh.authorizedKeys.keys = {SeedPlaceholders.UserAuthorizedKeys};\n" +
            $"    initialPassword = {SeedPlaceholders.UserInitialPassword};" +
            $"}};";

        var testContent =
            $"userNameSet = {{ expr = \"{SeedPlaceholders.UserName}\" != \"\"; expected = true; }};";

        return BuildTemplate(
            ownerId,
            templateId,
            "Regular User",
            ModuleType.Generic,
            content,
            definitions,
            "regular-user-test",
            testContent,
            [
                SeedPlaceholders.UserName, SeedPlaceholders.UserDescription, SeedPlaceholders.UserGroups,
                SeedPlaceholders.UserAuthorizedKeys, SeedPlaceholders.UserInitialPassword
            ]
        );
    }
}