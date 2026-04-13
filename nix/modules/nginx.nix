{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  prom = cfg.monitoring.prometheusServer;
in
{
  config = lib.mkIf (cfg.enable && cfg.nginx.enable) {
    services.nginx = {
      enable = true;
      recommendedProxySettings = true;
      virtualHosts.${cfg.nginx.hostName} = {
        locations = {
          "/" = { 
            root = "${cfg.webapp.package}/lib/PhoeNix.WebAPP/wwwroot"; 
            extraConfig = "try_files $uri $uri/ /index.html;"; 
          };

          "/api/" = lib.mkIf cfg.nginx.proxyApi.enable { 
            proxyPass = lib.removeSuffix "/" cfg.nginx.proxyApi.upstream; 
            proxyWebsockets = true; 
          };
          
          "/prometheus/" = lib.mkIf (prom.enable && prom.ui.nginxProxy) { 
            proxyPass = "http://127.0.0.1:${toString prom.port}/"; 
          };
        };
      };
    };
    networking.firewall.allowedTCPPorts = [ 80 443 ];
  };
}