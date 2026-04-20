{ self, lib, pkgs }:
{ config, ... }:
let
  cfg = config.services.phoenix;
in
{
  options.services.phoenix = {
    enable = lib.mkEnableOption "Phoenix orchestrator stack";
    user = lib.mkOption { type = lib.types.str; default = "phoenix"; };
    group = lib.mkOption { type = lib.types.str; default = "phoenix"; };
    stateDir = lib.mkOption { type = lib.types.str; default = "/var/lib/phoenix"; };

    api = {
      package = lib.mkOption { type = lib.types.package; default = self.packages.${pkgs.stdenv.hostPlatform.system}.webapi; };
      program = lib.mkOption { type = lib.types.nullOr lib.types.str; default = null; };
      urls = lib.mkOption { type = lib.types.str; default = "http://127.0.0.1:5001"; };
      environment = lib.mkOption { type = lib.types.attrsOf lib.types.str; default = { }; };
      environmentFile = lib.mkOption {
        type = lib.types.nullOr lib.types.str;
        default = null;
        description = "Path to an environment file containing secrets (not stored in Nix store).";
      };
      extraArgs = lib.mkOption { type = lib.types.listOf lib.types.str; default = [ ]; };
    };

    webapp.package = lib.mkOption { type = lib.types.package; default = self.packages.${pkgs.stdenv.hostPlatform.system}.webapp; };

    mcpServer = {
      enable = lib.mkOption { type = lib.types.bool; default = true; };
      package = lib.mkOption { type = lib.types.package; default = self.packages.${pkgs.stdenv.hostPlatform.system}.mcpserver; };
      program = lib.mkOption { type = lib.types.nullOr lib.types.str; default = null; };
      urls = lib.mkOption { type = lib.types.str; default = "http://127.0.0.1:5003"; };
      environment = lib.mkOption { type = lib.types.attrsOf lib.types.str; default = { }; };
      environmentFile = lib.mkOption {
        type = lib.types.nullOr lib.types.str;
        default = null;
        description = "Path to an environment file containing secrets (not stored in Nix store).";
      };
      extraArgs = lib.mkOption { type = lib.types.listOf lib.types.str; default = [ ]; };
    };

    database = {
      createLocally = lib.mkOption { type = lib.types.bool; default = true; };
      name = lib.mkOption { type = lib.types.str; default = "phoenix"; };
      user = lib.mkOption { type = lib.types.str; default = cfg.user; };
    };

    monitoring = {
      enable = lib.mkEnableOption "Monitoring stack";
      prometheusServer = {
        enable = lib.mkEnableOption "Prometheus server";
        port = lib.mkOption { type = lib.types.port; default = 9090; };
        tokenFile = lib.mkOption {
          type = lib.types.str;
          default = "${cfg.stateDir}/prometheus-token";
          description = "Path where API generates and Prometheus reads the Bearer token.";
        };
        ui = {
          public = lib.mkOption { type = lib.types.bool; default = false; };
          nginxProxy = lib.mkOption { type = lib.types.bool; default = false; };
        };
        httpDiscovery = {
          enable = lib.mkOption { type = lib.types.bool; default = true; };
          endpoint = lib.mkOption { type = lib.types.str; default = "http://127.0.0.1:5001/api/monitoring/targets"; };
          refreshInterval = lib.mkOption { type = lib.types.str; default = "1m"; };
        };
      };
      nodeExporter = {
        enable = lib.mkOption { type = lib.types.bool; default = true; };
        port = lib.mkOption { type = lib.types.port; default = 9100; };
      };
    };

    nginx = {
      enable = lib.mkEnableOption "nginx reverse proxy";
      hostName = lib.mkOption { type = lib.types.str; default = "phoenix.local"; };
      proxyApi = {
        enable = lib.mkOption { type = lib.types.bool; default = true; };
        upstream = lib.mkOption { type = lib.types.str; default = "http://127.0.0.1:5001"; };
      };
    };

    pixiecore = {
      enableWrapper = lib.mkOption { type = lib.types.bool; default = false; };
      wrapperName = lib.mkOption { type = lib.types.str; default = "pixiecore"; };
      package = lib.mkOption { type = lib.types.nullOr lib.types.package; default = pkgs.pixiecore; };
    };
  };
}