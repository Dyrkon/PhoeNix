{ inputs, pkgs, config, lib, ... }:
{
  imports = [
    inputs.self.nixosModules.default
  ];

  users.users.phoenix-admin = {
    isNormalUser = true;
    extraGroups = [ "wheel" "phoenix" ]; 
    
    initialPassword = "phoenix-default-pass";
    
    openssh.authorizedKeys.keys = [
    ];
  };

  security.sudo.wheelNeedsPassword = false;

  services.openssh = {
    enable = true;
    settings = {
      PermitRootLogin = "yes";
      PasswordAuthentication = true;
    };
  };

  services.avahi = {
    enable = true;
    nssmdns4 = true;
    openFirewall = true;
    publish = {
      enable = true;
      addresses = true;
      workstation = true;
    };
  };

  services.udev.extraRules = ''
    ACTION=="add", SUBSYSTEM=="net", KERNEL=="br[0-9]*|vmbr[0-9]*|docker[0-9]*", ATTR{bridge/multicast_snooping}="0"
  '';

  nix.settings = {
    experimental-features = [ "nix-command" "flakes" ];
    trusted-users = [ "root" "phoenix" "phoenix-admin" ];
    sandbox = lib.mkIf config.boot.isContainer false;
    substituters = [ "https://cache.nixos.org/" "https://nix-community.cachix.org" ];
    trusted-public-keys = [
      "cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY="
      "nix-community.cachix.org-1:mB9FSh9qf2dCimDSUo8Zy7bkq5CX+/rkCWyvRCUSeBo="
    ];
  };

  boot.loader.systemd-boot.enable = lib.mkIf (!config.boot.isContainer) true;
  boot.loader.efi.canTouchEfiVariables = lib.mkIf (!config.boot.isContainer) true;
  services.qemuGuest.enable = lib.mkIf (!config.boot.isContainer) true;
  
  boot.initrd.availableKernelModules = lib.mkIf (!config.boot.isContainer) [ 
    "virtio_pci" "virtio_blk" "virtio_scsi" "virtio_net" "virtio_balloon" "virtio_console" 
  ];

  services.phoenix = {
    enable = true;
    api.environment = {
      "Logging__LogLevel__Default" = "Information";
      "Logging__LogLevel__Microsoft.AspNetCore" = "Warning";

      "NetbootHost__HostExecutablePath" = "/run/wrappers/bin/pixiecore";

      "SeedExample__HostName" = "phoenix-orchestrator";
      "SeedExample__StateVersion" = "25.11";
      "SeedExample__RootAuthorizedKeys__0" = "ssh-ed25519 YOUR KEY";
      "SeedExample__PublicBaseUrl" = "http://YOUR-API-OR-HOSTNAME";
      "SeedExample__MetricsPort" = "9100";
      "SeedExample__OpenFirewall" = "true";
      "Cors__AllowedOrigins__0" = "https://YOUR-API-OR-HOSTNAME";
    };
    monitoring = {
      enable = true;
      prometheusServer = {
        enable = true;
        ui = {
          public = true;
          nginxProxy = true;
        };
      };
      nodeExporter.enable = true;
    };

    nginx.enable = true;

    mcpServer.enable = true;
  };

  networking.hostName = "phoenix-orchestrator";
  time.timeZone = "UTC";
  system.stateVersion = "25.11";
}