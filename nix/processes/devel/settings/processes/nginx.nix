{ self', pkgs, ... }:
let
  # This will run on 80 in production
  facePort = 8000;
  apiPort = 5083;
  webPort = 5000;
  stateDir = ".process-state/nginx";
  jobFilesDirectoryPath = "../files";
  config = pkgs.writeText "nginx.conf" ''
    pid nginx.pid;

    daemon off;

    events { }
    http {
      include ${pkgs.nginx}/conf/mime.types;

      client_body_temp_path tmp/client_body;
      proxy_temp_path tmp/proxy;
      fastcgi_temp_path tmp/fastcgi;
      scgi_temp_path tmp/scgi;
      uwsgi_temp_path tmp/uwsgi;

      access_log log/access.log;

      server {
        server_name localhost;
        listen ${toString facePort};

        location / {
          proxy_pass http://localhost:${toString webPort}/;
        }

        location /api/ {
          proxy_pass http://localhost:${toString apiPort}/;
          proxy_set_header Host $host;
          proxy_set_header X-Real-IP $remote_addr;
          proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        }
      }
    }
  '';
  command = pkgs.writeShellScriptBin "nginx" ''
    if [ ! -d ${stateDir} ]; then
      mkdir -p ${stateDir}/{log,tmp,www}
      chmod -R 750 ${stateDir}
    fi
    ${pkgs.lib.getExe pkgs.nginx} -c ${config} -p $(pwd)/${stateDir} -e log/error.log
  '';
  readinessProbe = pkgs.writeShellScript "api-ready.sh" ''
    ${pkgs.lib.getExe pkgs.curl} -sf localhost:${toString facePort}
  '';
in
{
  inherit command;
  readiness_probe = {
    exec.command = "${readinessProbe}";
    initial_delay_seconds = 2;
    period_seconds = 2;
    failure_threshold = 100;
  };
}
