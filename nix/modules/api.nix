{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  apiExec = if cfg.api.program != null then cfg.api.program else lib.getExe cfg.api.package;
in
{
  config = lib.mkIf cfg.enable {
    systemd.services.phoenix-api = {
      description = "Phoenix Web API";
      after = [ "network.target" "phoenix-api-env.service" ]
        ++ lib.optional cfg.database.createLocally "postgresql.service";
      requires = [ "phoenix-api-env.service" ]
        ++ lib.optional cfg.database.createLocally "postgresql.service";
      wantedBy = [ "multi-user.target" ];

      environment = cfg.api.environment // {
        ASPNETCORE_URLS = cfg.api.urls;
        PHOENIX_STATE_DIR = cfg.stateDir;
        PHOENIX_PROMETHEUS_TOKEN_PATH = cfg.monitoring.prometheusServer.tokenFile;
      } // lib.optionalAttrs cfg.monitoring.prometheusServer.enable {
        Monitoring__PrometheusEndpoint = "http://127.0.0.1:${toString cfg.monitoring.prometheusServer.port}/prometheus";
      };

      preStart = ''
        TOKEN_PATH="${cfg.monitoring.prometheusServer.tokenFile}"
        mkdir -p "$(dirname "$TOKEN_PATH")"
        if [ ! -s "$TOKEN_PATH" ]; then
          tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 32 > "$TOKEN_PATH"
        fi
        chown ${cfg.user}:${cfg.group} "$TOKEN_PATH"
        chmod 0640 "$TOKEN_PATH"
      '';

      serviceConfig = {
        User = cfg.user;
        Group = cfg.group;
        StateDirectory = "phoenix";
        WorkingDirectory = cfg.stateDir;
        Restart = "on-failure";
        ExecStart = "${apiExec} ${lib.concatStringsSep " " (map lib.escapeShellArg cfg.api.extraArgs)}";
      } // lib.optionalAttrs (cfg.api.environmentFile != null) {
        EnvironmentFile = cfg.api.environmentFile;
      };
    };
  };
}