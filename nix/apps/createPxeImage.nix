{
  pkgs,
  lib,
  inputs,
  project,
  phoenixUserCaPublicKey ? null,
}:
let
  systemArch = pkgs.stdenv.hostPlatform.system;

  systemConfiguration = inputs.nixpkgs.lib.nixosSystem {
    system = systemArch;
    modules = [
      ({ config, modulesPath, ... }: {
        imports = [
          (modulesPath + "/installer/netboot/netboot-minimal.nix")
        ];

        services.openssh.enable = true;

        services.openssh.settings = {
          TrustedUserCAKeys = "/etc/ssh/phoenix_user_ca.pub";
          PermitRootLogin = "prohibit-password";
          PasswordAuthentication = false;
          KbdInteractiveAuthentication = false;
          PubkeyAuthentication = true;

          X11Forwarding = false;
          PermitTunnel = false;
          AllowAgentForwarding = false;
          AllowTcpForwarding = true;
        };

        environment.etc."ssh/phoenix_user_ca.pub".text = phoenixUserCaPublicKey;

        environment.etc."ssh/root_authorized_principals".text = ''
          root
        '';

        services.openssh.extraConfig = ''
          Match User root
            AuthorizedPrincipalsFile /etc/ssh/root_authorized_principals
        '';

        users.users.root.openssh.authorizedKeys.keys = lib.mkForce [ ];

        config.system.stateVersion = config.system.nixos.release;
      })
    ];
  };

  build = systemConfiguration.config.system.build;

  json = builtins.toJSON {
    kernel = "${build.kernel}/bzImage";
    ramDisk = "${build.netbootRamdisk}/initrd";
    init = "${build.toplevel}/init";
    system = "${systemArch}";
  };
in
{
  type = "app";
  meta.description = "Create PXE bootstrap image with SSH CA trust";

  program = "${pkgs.writeShellScript "create-pxe-image" ''
    set -euo pipefail
    echo '${json}'
  ''}";
}