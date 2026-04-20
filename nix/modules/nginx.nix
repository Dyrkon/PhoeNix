{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  prom = cfg.monitoring.prometheusServer;
in
{
  config = lib.mkIf (cfg.enable && cfg.nginx.enable) {
    networking.firewall.allowedTCPPorts = [ 80 443 ];

    services.nginx =
      let
        tls-cert = pkgs.runCommand "phoenix-selfSignedCert" { buildInputs = [ pkgs.openssl ]; } ''
          mkdir -p $out
          openssl req -x509 -newkey ec -pkeyopt ec_paramgen_curve:secp384r1 -days 365 -nodes \
            -keyout $out/cert.key -out $out/cert.crt \
            -subj "/CN=${cfg.nginx.hostName}" -addext "subjectAltName=DNS:${cfg.nginx.hostName},IP:127.0.0.1"
        '';
      in
      {
        enable = true;
        
        recommendedGzipSettings = true;
        recommendedBrotliSettings = true;
        recommendedOptimisation = true;
        recommendedProxySettings = true;

        virtualHosts.${cfg.nginx.hostName} = {
          addSSL = true;
          
          sslCertificate = "${tls-cert}/cert.crt";
          sslCertificateKey = "${tls-cert}/cert.key";

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

          locations = lib.mkMerge [
            {
              "/" = {
                root = "${cfg.webapp.package}";
                tryFiles = "$uri $uri/ /index.html =404";
              };
            }

            (lib.mkIf cfg.nginx.proxyApi.enable {
              "/api/" = {
                recommendedProxySettings = true;
                proxyPass = lib.removeSuffix "/" cfg.nginx.proxyApi.upstream;
                proxyWebsockets = true;
                extraConfig = ''
                  proxy_cookie_path / /;
                '';
              };
            })

            (lib.mkIf (prom.enable && prom.ui.nginxProxy) {
              "/prometheus/" = {
                recommendedProxySettings = true;
                proxyPass = "http://127.0.0.1:${toString prom.port}";
                proxyWebsockets = true;
              };
            })

            (lib.mkIf cfg.mcpServer.enable {
              "/mcp/" = {
                recommendedProxySettings = true;
                proxyPass = lib.removeSuffix "/" cfg.mcpServer.urls;
                proxyWebsockets = true;
              };
            })
          ];
        };
      };
  };
}