{ pkgs
, nixpkgs'
, ... }:
let
  pname = "pxe-starter";
  sshKeys = "";
  supportedSystems = [ "x86_64-linux" "aarch64-linux" ];

  systemConfigurations = builtins.listToAttrs (map (system:
    {
      name = system;
      value = nixpkgs'.lib.nixosSystem {
        system = system;
        modules = [
          ({ config, pkgs, lib, modulesPath, ... }: {
            imports = [
              (modulesPath + "/installer/netboot/netboot-minimal.nix")
            ];
            config = {
              users.users.root.openssh.authorizedKeys.keys = [ sshKeys ];
              system.stateVersion = config.system.nixos.release;
            };
          })
        ];
      };
    }
  ) supportedSystems);

  createPackage = configuration:
    let
      build = configuration.config.system.build;
      architecture = configuration.pkgs.system;
      scriptName = "${pname}-${architecture}";
    in
      pkgs.stdenv.mkDerivation {
        name = scriptName;
        version = "0.0";
        src = ./.;
        installPhase = ''
          mkdir -p $out/bin
          cat > $out/bin/${scriptName} <<EOF
          exec ${pkgs.pixiecore}/bin/pixiecore \
            boot ${build.kernel}/bzImage ${build.netbootRamdisk}/initrd \
            --cmdline "init=${build.toplevel}/init loglevel=4" \
            --debug --dhcp-no-bind \
            --port 64172 --status-port 64172 "\$\@"
          EOF
          chmod +x $out/bin/${scriptName}
        '';

        meta = with pkgs.lib; {
          description = "PXE starter script for netboot on ${architecture} architecture";
          platforms = supportedSystems;
        };
      };

  packages = builtins.listToAttrs (map (systemConfig: {
    name = "${pname}-${systemConfig.pkgs.system}";
    value = createPackage systemConfig;
  }) (builtins.attrValues systemConfigurations));
in
  packages

