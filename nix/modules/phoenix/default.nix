{ self }:
{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  envFile = "${cfg.stateDir}/environment";
in
{
  imports = [
    (import ../options.nix { inherit self lib pkgs; })
    ../api.nix
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
        EOF
        fi

        chown ${cfg.user}:${cfg.group} "$ENV_FILE"
        chmod 0600 "$ENV_FILE"
      '';
    };
  };
}