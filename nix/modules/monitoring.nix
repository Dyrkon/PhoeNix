{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  mon = cfg.monitoring;
  prom = mon.prometheusServer;
  runtimeToken = "${cfg.stateDir}/prometheus-token";

  prometheusYml = pkgs.writeText "prometheus.yml" (builtins.toJSON {
    global.scrape_interval = "15s";
    scrape_configs =
      lib.optional prom.httpDiscovery.enable {
        job_name = "orchestrated-machines";
        http_sd_configs = [{
          url = prom.httpDiscovery.endpoint;
          refresh_interval = prom.httpDiscovery.refreshInterval;
          authorization = {
            type = "Bearer";
            credentials_file = runtimeToken;
          };
        }];
      };
  });
in
{
  config = lib.mkIf (cfg.enable && mon.enable) (lib.mkMerge [
    {
      system.activationScripts.phoenix-token = ''
        mkdir -p ${cfg.stateDir}
        touch ${runtimeToken}
      '';
    }

    {
      networking.firewall.allowedTCPPorts =
        lib.optional (prom.enable && prom.ui.public) prom.port;
    }

    (lib.mkIf prom.enable {
      users.users.prometheus.extraGroups = [ cfg.group ];

      services.prometheus = {
        enable = true;
        port = prom.port;
        checkConfig = false;
        configText = builtins.readFile prometheusYml;
        webExternalUrl = "/prometheus/";
      };

      systemd.services.prometheus = {
        after = [ "phoenix-api.service" ];
        requires = [ "phoenix-api.service" ];
        preStart = ''
          while [ ! -s "${runtimeToken}" ]; do
            sleep 1
          done
        '';
      };
    })

    (lib.mkIf mon.nodeExporter.enable {
      services.prometheus.exporters.node = {
        enable = true;
        port = mon.nodeExporter.port;
        enabledCollectors = [ "systemd" ];
        openFirewall = true;
      };
    })
  ]);
}