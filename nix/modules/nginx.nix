{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  prom = cfg.monitoring.prometheusServer;
  tlsCertDir = "/var/lib/phoenix/tls";
in
{
  config = lib.mkIf (cfg.enable && cfg.nginx.enable) {
    networking.firewall.allowedTCPPorts = [ 80 443 ];

    systemd.services.phoenix-tls-cert = {
      description = "Generate PhoeNix self-signed TLS certificate";
      before = [ "nginx.service" ];
      wantedBy = [ "nginx.service" ];
      serviceConfig = {
        Type = "oneshot";
        RemainAfterExit = true;
      };
      script = ''
        set -euo pipefail
        mkdir -p ${tlsCertDir}

        # Skip if cert exists and is valid for more than 7 days
        if [ -f "${tlsCertDir}/cert.crt" ]; then
          if ${pkgs.openssl}/bin/openssl x509 -checkend 604800 -noout -in "${tlsCertDir}/cert.crt" 2>/dev/null; then
            exit 0
          fi
        fi

        HOSTNAME=$(${pkgs.inetutils}/bin/hostname -f)
        SHORT=$(${pkgs.inetutils}/bin/hostname -s)
        IP=$(${pkgs.iproute2}/bin/ip route get 1 2>/dev/null | ${pkgs.gawk}/bin/awk '{for(i=1;i<=NF;i++) if($i=="src") {print $(i+1); exit}}' || echo "")

        SAN="DNS:$HOSTNAME,DNS:$SHORT,DNS:localhost,IP:127.0.0.1"
        if [ -n "$IP" ] && [ "$IP" != "127.0.0.1" ]; then
          SAN="$SAN,IP:$IP"
        fi

        ${pkgs.openssl}/bin/openssl req -x509 -newkey ec \
          -pkeyopt ec_paramgen_curve:secp384r1 -days 365 -nodes \
          -keyout "${tlsCertDir}/cert.key" -out "${tlsCertDir}/cert.crt" \
          -subj "/CN=$HOSTNAME" -addext "subjectAltName=$SAN"

        chmod 640 "${tlsCertDir}/cert.key"
        chown root:nginx "${tlsCertDir}/cert.key"
      '';
    };

    services.nginx =
      {
        enable = true;

        recommendedGzipSettings = true;
        recommendedBrotliSettings = true;
        recommendedOptimisation = true;
        recommendedProxySettings = true;

        virtualHosts.${cfg.nginx.hostName} = {
          addSSL = true;

          sslCertificate = "${tlsCertDir}/cert.crt";
          sslCertificateKey = "${tlsCertDir}/cert.key";

          extraConfig = ''
            if ($scheme = http) {
              set $do_redirect "yes";
            }
            if ($request_uri ~ ^(/api/(v1/boot|setup/bootstrap/callback|setup/finalize|provisioning/files)|/mcp|/oauth|/\.well-known/oauth)) {
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
              "/oauth/" = {
                recommendedProxySettings = true;
                proxyPass = lib.removeSuffix "/" cfg.mcpServer.urls;
              };
              "/.well-known/oauth-protected-resource" = {
                recommendedProxySettings = true;
                proxyPass = lib.removeSuffix "/" cfg.mcpServer.urls;
              };
              "/.well-known/oauth-authorization-server" = {
                recommendedProxySettings = true;
                proxyPass = lib.removeSuffix "/" cfg.mcpServer.urls;
              };
            })
          ];
        };
      };
  };
}