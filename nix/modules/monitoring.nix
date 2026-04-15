{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  mon = cfg.monitoring;
  prom = mon.prometheusServer;
in
{
  config = lib.mkIf (cfg.enable && mon.enable) (lib.mkMerge [
    {
      networking.firewall.allowedTCPPorts =
        lib.optional (prom.enable && prom.ui.public) prom.port;
    }

    (lib.mkIf prom.enable {
      users.users.prometheus.extraGroups = [ cfg.group ];

      systemd.services.prometheus = {
        after = [ "phoenix-api.service" ];
        requires = [ "phoenix-api.service" ];
      };

      services.prometheus = {
        enable = true;
        port = prom.port;

        scrapeConfigs = lib.optional prom.httpDiscovery.enable {
          job_name = "orchestrated-machines";
          http_sd_configs = lib.singleton {
            url = prom.httpDiscovery.endpoint;
            refresh_interval = prom.httpDiscovery.refreshInterval;
            authorization = {
              type = "Bearer";
              credentials_file = prom.tokenFile;
            };
          };
        };
      };
    })

    (lib.mkIf mon.nodeExporter.enable {
      services.prometheus.exporters.node = {
        enable = true;
        port = mon.nodeExporter.port;
        enabledCollectors = [ "systemd" ];
      };
    })

  ]);
}