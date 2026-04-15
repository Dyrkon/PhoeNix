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

      "SeedExample__HostName" = "phoenix-demo";
      "SeedExample__StateVersion" = "25.11";
      "SeedExample__RootAuthorizedKeys__0" = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIBogRs9tt7sCKyEM+Vj16pM8tTesXTPWh5nA5lvOc6kM dyrkon603@gmail.com";
      "SeedExample__PublicBaseUrl" = "http://192.168.88.144";
      "SeedExample__MetricsPort" = "9100";
      "SeedExample__OpenFirewall" = "true";
      "Cors__AllowedOrigins__0" = "https://192.168.88.144";
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
  };

  networking.hostName = "phoenix-orchestrator";
  time.timeZone = "UTC";
  system.stateVersion = "25.11";
}