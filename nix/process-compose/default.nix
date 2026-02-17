{
  pkgs,
  lib,
  project,
}: let
  runDir = "/tmp/phoenix-nginx";
  nginxConf = pkgs.writeText "nginx.conf" ''
    worker_processes  1;
    pid ${runDir}/nginx.pid;

    events { worker_connections  1024; }

    error_log  ${runDir}/error.log info;

    http {
      include       ${pkgs.nginx}/conf/mime.types;
      default_type  application/octet-stream;
      sendfile        on;

      client_body_temp_path /tmp/phoenix-nginx/client_body;
      proxy_temp_path       /tmp/phoenix-nginx/proxy;
      fastcgi_temp_path     /tmp/phoenix-nginx/fastcgi;
      uwsgi_temp_path       /tmp/phoenix-nginx/uwsgi;
      scgi_temp_path        /tmp/phoenix-nginx/scgi;


      access_log ${runDir}/access.log;

      upstream webapi { server 127.0.0.1:5001; }
      upstream webapp { server 127.0.0.1:5002; }

      server {
        listen 8888;

        location /api/ {
          proxy_pass http://webapi/;
          proxy_set_header Host $host;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;
        }

        location / {
          proxy_pass http://webapp/;
          proxy_set_header Host $host;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
          proxy_set_header X-Forwarded-Proto $scheme;
        }
      }
    }
  '';

  pcYaml = pkgs.writeText "process-compose.yaml" ''
    version: "0.5"
    processes:
      webapi:
        command: "${project.dotnetSdk}/bin/dotnet run --project ./sources/src/PhoeNix.WebAPI/PhoeNix.WebAPI.csproj --configuration Release"
        availability:
          restart: on_failure
        environment:
          - ASPNETCORE_URLS=http://127.0.0.1:5001

      webapp:
        command: "${project.dotnetSdk}/bin/dotnet run --project ./sources/src/PhoeNix.WebAPP/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj --configuration Release"
        availability:
          restart: on_failure
        environment:
          - ASPNETCORE_URLS=http://127.0.0.1:5002
        depends_on:
          webapi:
            condition: process_started

      nginx:
        command: "${pkgs.writeShellScript "run-nginx" ''
          set -euo pipefail
          runDir=/tmp/phoenix-nginx
          mkdir -p "$runDir" \
            "$runDir/client_body" "$runDir/proxy" "$runDir/fastcgi" "$runDir/uwsgi" "$runDir/scgi"

          exec ${pkgs.nginx}/bin/nginx \
            -c ${nginxConf} \
            -g "daemon off;"
        ''}"
        availability:
          restart: on_failure
        depends_on:
          webapp:
            condition: process_started
  '';
in {
  configPackage = pkgs.runCommand "phoenix-process-compose" {} ''
    mkdir -p $out
    cp ${pcYaml} $out/process-compose.yaml
    cp ${nginxConf} $out/nginx.conf
  '';
}
