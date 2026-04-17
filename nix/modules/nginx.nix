{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  prom = cfg.monitoring.prometheusServer;
in
{
  config = lib.mkIf (cfg.enable && cfg.nginx.enable) {
    system.activationScripts.phoenix-cert = ''
      mkdir -p /var/lib/phoenix
      if [ ! -f /var/lib/phoenix/cert.pem ]; then
        ${pkgs.openssl}/bin/openssl req -x509 -newkey rsa:4096 \
          -keyout /var/lib/phoenix/cert.key \
          -out /var/lib/phoenix/cert.pem \
          -days 365 -nodes \
          -subj "/CN=${cfg.nginx.hostName}"
      fi
      chown nginx:nginx /var/lib/phoenix/cert.pem /var/lib/phoenix/cert.key
      chmod 0640 /var/lib/phoenix/cert.key
      chmod 0644 /var/lib/phoenix/cert.pem
    '';

    services.nginx = {
      enable = true;
      recommendedProxySettings = true;

      virtualHosts.${cfg.nginx.hostName} = {
        addSSL = true;
        sslCertificate = "/var/lib/phoenix/cert.pem";
        sslCertificateKey = "/var/lib/phoenix/cert.key";

        extraConfig = ''
          if ($scheme = http) {
            set $do_redirect "yes";
          }
          if ($request_uri ~ ^/api/(v1/boot|setup/bootstrap/callback|setup/finalize|provisioning/files)) {
            set $do_redirect "no";
          }
          if ($do_redirect = "yes") {
            return 301 https://$host$request_uri;
          }
        '';

        locations = {
          "/" = {
            root = "${cfg.webapp.package}";
            extraConfig = "try_files $uri $uri/ /index.html;";
          };

          "/api/" = lib.mkIf cfg.nginx.proxyApi.enable {
            proxyPass = lib.removeSuffix "/" cfg.nginx.proxyApi.upstream;
            proxyWebsockets = true;
            extraConfig = ''
              proxy_set_header Host $host;
              proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
              proxy_set_header X-Forwarded-Proto $scheme;
              proxy_set_header X-Forwarded-Host $host;
              proxy_cookie_path / /;
            '';
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