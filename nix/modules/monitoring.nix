{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  mon = cfg.monitoring;
  prom = mon.prometheusServer;
in
{
  config = lib.mkIf (cfg.enable && mon.enable) {
    users.users.prometheus.extraGroups = [ cfg.group ];

    systemd.services.prometheus = {
      after = [ "phoenix-api.service" ];
      requires = [ "phoenix-api.service" ];
    };

    services.prometheus = lib.mkIf prom.enable {
      enable = true;
      port = prom.port;
      
      scrapeConfigs = [
        {
          job_name = "orchestrated-machines";
          http_sd_configs = lib.singleton {
            url = prom.httpDiscovery.endpoint;
            refresh_interval = prom.httpDiscovery.refreshInterval;
            authorization = {
              type = "Bearer";
              credentials_file = prom.tokenFile;
            };
          };
        }
      ];
    };

    services.prometheus.exporters.node = lib.mkIf mon.nodeExporter.enable {
      enable = true;
      port = mon.nodeExporter.port;
      enabledCollectors = [ "systemd" ];
    };

    networking.firewall.allowedTCPPorts = 
      lib.optional (prom.enable && prom.ui.public) prom.port;
  };
}