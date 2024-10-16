{pkgs, ...}: let
  port = 5000;
  command = pkgs.lib.getExe inputs.self.packages.${pkgs.system}.webapp;
  readinessProbe = pkgs.writeShellScript "app-ready.sh" ''
    ${pkgs.lib.getExe pkgs.curl} -sf localhost:${toString port}
  '';
in {
  inherit command;
  environment = [];
  depends_on = {
    webapi.condition = "process_healthy";
  };
  readiness_probe = {
    exec.command = "${readinessProbe}";
    initial_delay_seconds = 5;
    period_seconds = 10;
    failure_threshold = 3;
  };
}
