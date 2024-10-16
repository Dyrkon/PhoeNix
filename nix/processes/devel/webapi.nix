{pkgs, ...}: let
  port = 5083;
  readinessProbe = pkgs.writeShellScript "api-ready.sh" ''
    ${pkgs.lib.getExe pkgs.curl} -sf localhost:${toString port}/health
  '';
in {
  command = pkgs.lib.getExe inputs.self.packages.${pkgs.system}.webapi;
  environment = ["ASPNETCORE_URLS=http://*:${toString port}"];
  depends_on = {
    # db.condition = "process_healthy";
  };
  readiness_probe = {
    exec.command = "${readinessProbe}";
    initial_delay_seconds = 2;
    period_seconds = 2;
    failure_threshold = 20;
  };
}
