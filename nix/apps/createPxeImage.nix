{
  pkgs,
  lib,
  inputs,
  project,
}:
let
  systemArch = pkgs.stdenv.hostPlatform.system;

  systemConfiguration = inputs.nixpkgs.lib.nixosSystem {
    system = systemArch;
    modules = [
      ({ config, modulesPath, ... }: {
        imports = [(modulesPath + "/installer/netboot/netboot-minimal.nix")];
        config.system.stateVersion = config.system.nixos.release;
      })
    ];
  };

  build = systemConfiguration.config.system.build;

  json = builtins.toJSON {
    kernel = "${build.kernel}/bzImage";
    ramDisk = "${build.netbootRamdisk}/initrd";
    init = "${build.toplevel}/init";
    system = systemArch;
  };

in
{
  type = "app";
  meta.description = "Create PXE bootstrap image";

  program = "${pkgs.writeShellScript "updateDeps" ''
    set -euo pipefail
    echo '${json}'
  ''}";
}