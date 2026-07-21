{ self }:
{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  envFile = "${cfg.stateDir}/environment";
  createPxeImageScript = pkgs.writeShellScriptBin "phoenix-create-pxe-image" ''
    exec ${pkgs.nix}/bin/nix run "${self}#bootstrap" --impure "$@"
  '';
  toolsEnv = {
        PHOENIX_SSH_KEYGEN_PATH = "${pkgs.openssh}/bin/ssh-keygen";
        PHOENIX_SSH_PATH = "${pkgs.openssh}/bin/ssh";
        PHOENIX_NIX_PATH = "${pkgs.nix}/bin/nix";
        PHOENIX_NIXOS_REBUILD_PATH = "${pkgs.nixos-rebuild}/bin/nixos-rebuild";
        PHOENIX_NIXOS_ANYWHERE_PATH = "${pkgs.nixos-anywhere}/bin/nixos-anywhere";
        PHOENIX_DISKO_URL = "github:nix-community/disko/v1.12.0"; # 1.13.0 has an missmatch that prevents nixos-anywhere --vm-test from running
        PHOENIX_ALEJANDRA_PATH = "${pkgs.alejandra}/bin/alejandra";
        PHOENIX_DISKO_SOURCE_PATH = "github:nix-community/disko/v1.12.0";
      };
in
{
  imports = [
    (import ../options.nix { inherit self lib pkgs; })
    ../api.nix
    ../mcpserver.nix
    ../database.nix
    ../monitoring.nix
    ../nginx.nix
  ];

  config = lib.mkIf cfg.enable {
    users.groups.${cfg.group} = { };
    users.users.${cfg.user} = {
      isSystemUser = true;
      group = cfg.group;
      home = cfg.stateDir;
    };

    services.phoenix.api.environmentFile = lib.mkDefault envFile;

    services.phoenix.api.environment = lib.mkMerge [
      (lib.mkIf cfg.database.createLocally {
        "ConnectionStrings__PhoeNix" = "Host=/run/postgresql;Username=${cfg.database.user};Database=${cfg.database.name};Maximum Pool Size=20;";
      })
    ];

    services.phoenix.mcpServer.environmentFile = lib.mkDefault envFile;

    services.phoenix.mcpServer.environment = lib.mkMerge [
      (lib.mkIf cfg.database.createLocally {
        "ConnectionStrings__PhoeNix" = "Host=/run/postgresql;Username=${cfg.database.user};Database=${cfg.database.name};Maximum Pool Size=20;";
      })
    ];

    security.wrappers.pixiecore = {
      owner = "root";
      group = "root";
      source = "${pkgs.pixiecore}/bin/pixiecore";
      capabilities = "cap_net_raw,cap_net_bind_service+ep";
      permissions = "0755";
      setuid = false;
      setgid = false;
    };

    systemd.services.phoenix-api = {
      path = [
        pkgs.nixos-anywhere
        pkgs.openssh
        pkgs.nix
        pkgs.nixos-rebuild
        pkgs.coreutils
        createPxeImageScript
        config.security.wrapperDir
      ] ++ lib.optionals cfg.virtualization.enable [
        pkgs.libvirt
        pkgs.virt-manager
      ];
      environment = toolsEnv // lib.optionalAttrs cfg.virtualization.enable {
        PHOENIX_VIRSH_PATH = "${pkgs.libvirt}/bin/virsh";
        PHOENIX_VIRT_INSTALL_PATH = "${pkgs.virt-manager}/bin/virt-install";
      };
    };

    systemd.services.phoenix-mcp = {
      path = [
        pkgs.nix
        createPxeImageScript
      ];
      environment = toolsEnv;
    };

    networking.firewall = {
      allowedUDPPorts = [ 67 69 4011 ];
      allowedTCPPorts = [ 64172 64173 ];
    };

    systemd.services.phoenix-api-env = {
      description = "Generate Phoenix API environment file";
      wantedBy = [ "phoenix-api.service" ];
      before = [ "phoenix-api.service" ];
      after = [ "local-fs.target" ];
      serviceConfig = {
        Type = "oneshot";
        RemainAfterExit = true;
        User = "root";
      };
      script = ''
        mkdir -p "${cfg.stateDir}"
        ENV_FILE="${envFile}"

        if [ ! -f "$ENV_FILE" ]; then
          SIGNING_KEY=$(${pkgs.openssl}/bin/openssl rand -base64 32)
          cat > "$ENV_FILE" <<EOF
        CallbackToken__SigningKey=$SIGNING_KEY
        CallbackToken__Issuer=phoenix
        CallbackToken__Audience=phoenix
        EOF
        fi

        chown ${cfg.user}:${cfg.group} "$ENV_FILE"
        chmod 0600 "$ENV_FILE"
      '';
    };
  };
}