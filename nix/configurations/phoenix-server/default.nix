{ inputs, pkgs, ... }:
{
  imports = [
    inputs.self.nixosModules.default
    ./disko.nix
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

  boot.loader.systemd-boot.enable = true;
  boot.loader.efi.canTouchEfiVariables = true;
  services.qemuGuest.enable = true;
  
  boot.initrd.availableKernelModules = [ 
    "virtio_pci" 
    "virtio_blk" 
    "virtio_scsi" 
    "virtio_net" 
    "virtio_balloon" 
    "virtio_console" 
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