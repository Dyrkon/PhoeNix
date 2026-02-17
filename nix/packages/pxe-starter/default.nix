{
  pkgs,
  lib,
  inputs,
  project,
}: let
  pname = "pxe-starter";
  sshKeys = "";
  supportedSystems = ["x86_64-linux" "aarch64-linux"];

  systemConfiguration = inputs.nixpkgs.lib.nixosSystem {
    system = pkgs.stdenv.hostPlatform.system;
    modules = [
      ({
        config,
        pkgs,
        lib,
        modulesPath,
        ...
      }: {
        imports = [(modulesPath + "/installer/netboot/netboot-minimal.nix")];
        config = {
          users.users.root.openssh.authorizedKeys.keys = [sshKeys];
          system.stateVersion = config.system.nixos.release;
        };
      })
    ];
  };

  build = systemConfiguration.config.system.build;
in
  pkgs.stdenv.mkDerivation {
    name = pname;
    version = builtins.readFile project.versionFile;
    src = ./.;

    installPhase = ''
      mkdir -p $out/bin
      cat > $out/bin/${pname} <<EOF
      exec ${pkgs.pixiecore}/bin/pixiecore \
        boot ${build.kernel}/bzImage ${build.netbootRamdisk}/initrd \
        --cmdline "init=${build.toplevel}/init loglevel=4" \
        --debug --dhcp-no-bind \
        --port 64172 --status-port 64172 "\$@"
      EOF
      chmod +x $out/bin/${pname}
    '';

    meta = with pkgs.lib; {
      description = "PXE starter script for netbooting NixOS";
      platforms = supportedSystems;
    };
  }
