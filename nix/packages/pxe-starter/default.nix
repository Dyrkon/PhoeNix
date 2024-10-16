{
  # Snowfall Lib provides a customized `lib` instance with access to your flake's library
  # as well as the libraries available from your flake's inputs.
  lib,
  # You also have access to your flake's inputs.
  inputs,
  # The namespace used for your flake, defaulting to "internal" if not set.
  namespace,
  # All other arguments come from NixPkgs. You can use `pkgs` to pull packages or helpers
  # programmatically or you may add the named attributes as arguments here.
  pkgs,
  stdenv,
  ...
}: let
  pname = "pxe-starter";
  sshKeys = "";
  supportedSystems = ["x86_64-linux" "aarch64-linux"];

  systemConfiguration = inputs.nixpkgs.lib.nixosSystem {
    system = pkgs.system;
    modules = [
      ({
        config,
        pkgs,
        lib,
        modulesPath,
        ...
      }: {
        imports = [
          (modulesPath + "/installer/netboot/netboot-minimal.nix")
        ];
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
    version = "0.0";
    src = ./.;
    installPhase = ''
      mkdir -p $out/bin
      cat > $out/bin/${pname} <<EOF
      exec ${pkgs.pixiecore}/bin/pixiecore \
        boot ${build.kernel}/bzImage ${build.netbootRamdisk}/initrd \
        --cmdline "init=${build.toplevel}/init loglevel=4" \
        --debug --dhcp-no-bind \
        --port 64172 --status-port 64172 "\$\@"
      EOF
      chmod +x $out/bin/${pname}
    '';

    meta = with pkgs.lib; {
      description = "PXE starter script for netbooting NixOS";
      platforms = supportedSystems;
    };
  }
