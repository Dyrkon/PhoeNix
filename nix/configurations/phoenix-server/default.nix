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
    monitoring = {
      enable = true;
      prometheusServer.enable = true;
      prometheusServer.ui.nginxProxy = true;
    };
    nginx.enable = true;
  };

  networking.hostName = "phoenix-orchestrator";
  time.timeZone = "UTC";
  system.stateVersion = "25.11";

  services.openssh.enable = true;
  services.openssh.settings.PermitRootLogin = "yes";
}