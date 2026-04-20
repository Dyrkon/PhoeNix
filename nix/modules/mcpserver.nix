{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
  mcpExec = if cfg.mcpServer.program != null then cfg.mcpServer.program else lib.getExe cfg.mcpServer.package;
in
{
  config = lib.mkIf (cfg.enable && cfg.mcpServer.enable) {
    systemd.services.phoenix-mcp = {
      description = "Phoenix MCP Server";
      after = [ "network.target" "phoenix-api-env.service" ]
        ++ lib.optional cfg.database.createLocally "postgresql.service";
      requires = [ "phoenix-api-env.service" ]
        ++ lib.optional cfg.database.createLocally "postgresql.service";
      wantedBy = [ "multi-user.target" ];

      environment = cfg.mcpServer.environment // {
        ASPNETCORE_URLS = cfg.mcpServer.urls;
      };

      serviceConfig = {
        User = cfg.user;
        Group = cfg.group;
        StateDirectory = "phoenix";
        WorkingDirectory = cfg.stateDir;
        Restart = "on-failure";
        ExecStart = "${mcpExec} ${lib.concatStringsSep " " (map lib.escapeShellArg cfg.mcpServer.extraArgs)}";
      } // lib.optionalAttrs (cfg.mcpServer.environmentFile != null) {
        EnvironmentFile = cfg.mcpServer.environmentFile;
      };
    };
  };
}
