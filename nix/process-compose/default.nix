{
  pkgs,
  lib,
  project,
}: let
  runDir = ".dev-data/nginx-run";
  devDataDir = ".dev-data"; 
  
  dbPort = 5432;
  dbUser = "phoenix";
  dbName = "phoenix";
  promPort = 9090;
  nodeExporterPort = 9100;

  pgPkg = pkgs.postgresql_18.withPackages (pp: [ pp.pgvector ]);
  
  nginxConf = pkgs.writeText "nginx.conf" ''
    worker_processes  1;
    pid nginx.pid;
    error_log error.log info;
    events { worker_connections  1024; }
    http {
      include       ${pkgs.nginx}/conf/mime.types;
      default_type  application/octet-stream;
      sendfile        on;
      
      client_body_temp_path client_body;
      proxy_temp_path       proxy;
      fastcgi_temp_path     fastcgi;
      uwsgi_temp_path       uwsgi;
      scgi_temp_path        scgi;
      access_log access.log;

      upstream webapi { server 127.0.0.1:5001; }
      upstream webapp { server 127.0.0.1:5002; }
      upstream prometheus { server 127.0.0.1:${toString promPort}; }

      server {
        listen 8888;
        location /api/ { 
            proxy_pass http://webapi; 
            proxy_set_header Host ''$host;
            proxy_set_header X-Forwarded-For ''$proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto ''$scheme;
        }
        location /prometheus/ { 
            proxy_pass http://prometheus; 
            proxy_set_header Host ''$host;
        }
        location = /prometheus { return 301 ''$scheme://''$http_host/prometheus/; }
        location / { proxy_pass http://webapp/; }
      }
    }
  '';

in {
  configPackage = pkgs.runCommand "phoenix-process-compose" {} ''
    mkdir -p $out
    cp ${pkgs.writeText "process-compose.yaml" ''
    version: "0.5"
    processes:
      build-all:
        command: "${project.dotnetSdk}/bin/dotnet build sources/src/PhoeNix.WebAPI/PhoeNix.WebAPI.csproj --configuration Release && ${project.dotnetSdk}/bin/dotnet build sources/src/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj --configuration Release"
        environment:
          - "DOTNET_CLI_TELEMETRY_OPTOUT=1"

      postgres:
        command: "${pkgs.writeShellScript "run-postgres" ''
          set -euo pipefail
          export PATH="${pgPkg}/bin:$PATH"
          DB_DIR="$(pwd)/${devDataDir}/db"
          if [ ! -d "$DB_DIR" ]; then
            initdb -D "$DB_DIR" --auth=trust
            pg_ctl -D "$DB_DIR" -o "-k /tmp -p ${toString dbPort} -h 127.0.0.1" start
            until pg_isready -h 127.0.0.1 -p ${toString dbPort} -d postgres; do sleep 1; done
            psql -h 127.0.0.1 -p ${toString dbPort} -d postgres -c "CREATE USER ${dbUser} WITH SUPERUSER;"
            psql -h 127.0.0.1 -p ${toString dbPort} -d postgres -c "CREATE DATABASE ${dbName} OWNER ${dbUser};"
            pg_ctl -D "$DB_DIR" stop
          fi
          exec postgres -D "$DB_DIR" -k /tmp -p ${toString dbPort} -h 127.0.0.1
        ''}"
        readiness_probe:
          exec:
            command: "${pgPkg}/bin/pg_isready -h 127.0.0.1 -p ${toString dbPort} -d postgres"
          initial_delay_seconds: 5

      webapi:
        command: "${pkgs.writeShellScript "run-api" ''
          set -euo pipefail
          mkdir -p "$(pwd)/${devDataDir}/prometheus"
          export PHOENIX_STATE_DIR="$(pwd)/${devDataDir}/prometheus"
          exec ${project.dotnetSdk}/bin/dotnet run \
            --project ./sources/src/PhoeNix.WebAPI/PhoeNix.WebAPI.csproj \
            --configuration Release \
            --no-build \
            --no-launch-profile
        ''}"
        environment:
          - "ASPNETCORE_ENVIRONMENT=Development"
          - "ASPNETCORE_URLS=http://0.0.0.0:5001"
          - "ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=${toString dbPort};Username=${dbUser};Database=${dbName};"
          - "Monitoring__PrometheusEndpoint=http://127.0.0.1:${toString promPort}/prometheus"
                
      node-exporter:
        command: "${pkgs.prometheus-node-exporter}/bin/node_exporter --web.listen-address=127.0.0.1:${toString nodeExporterPort}"

      prometheus:
        command: "${pkgs.writeShellScript "run-prometheus" ''
          set -euo pipefail
          PROM_DIR="$(pwd)/${devDataDir}/prometheus"
          TKN_FILE="$PROM_DIR/prometheus-token"
          CONF_FILE="$PROM_DIR/prometheus.yml"
          mkdir -p "$PROM_DIR"
          cat > "$CONF_FILE" <<EOF
global:
  scrape_interval: 15s
scrape_configs:
  - job_name: 'prometheus'
    metrics_path: '/prometheus/metrics'
    static_configs:
      - targets: ['127.0.0.1:${toString promPort}']
  - job_name: 'orchestrator-node'
    static_configs:
      - targets: ['127.0.0.1:${toString nodeExporterPort}']
  - job_name: 'orchestrated-machines'
    http_sd_configs:
      - url: 'http://127.0.0.1:5001/api/monitoring/targets'
        refresh_interval: 1m
        authorization:
          type: 'Bearer'
          credentials_file: '$TKN_FILE'
EOF
          while [ ! -f "$TKN_FILE" ]; do sleep 2; done
          exec ${pkgs.prometheus}/bin/prometheus \
            --config.file="$CONF_FILE" \
            --storage.tsdb.path="$PROM_DIR/data" \
            --web.listen-address="127.0.0.1:${toString promPort}" \
            --web.external-url="http://localhost:8888/prometheus/"
        ''}"
        depends_on:
          webapi:
            condition: process_started

      webapp:
        command: "${project.dotnetSdk}/bin/dotnet run --project ./sources/src/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj --configuration Release --no-build --launch-profile https -- --urls \"https://127.0.0.1:7052;http://127.0.0.1:5002\""
        depends_on:
          webapi:
            condition: process_started
          build-all:
            condition: process_completed

      nginx:
        command: "${pkgs.writeShellScript "run-nginx" ''
          set -euo pipefail
          REAL_RUN_DIR="$(pwd)/${runDir}"
          mkdir -p "$REAL_RUN_DIR/client_body" "$REAL_RUN_DIR/proxy" "$REAL_RUN_DIR/fastcgi" "$REAL_RUN_DIR/uwsgi" "$REAL_RUN_DIR/scgi"
          
          exec ${pkgs.nginx}/bin/nginx -p "$REAL_RUN_DIR" -c "${nginxConf}" -e "$REAL_RUN_DIR/error.log" -g "daemon off;"
        ''}"
        depends_on:
          webapp:
            condition: process_started
    ''} $out/process-compose.yaml
    cp ${nginxConf} $out/nginx.conf
  '';
}