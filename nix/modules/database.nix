{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
in
{
  config = lib.mkIf (cfg.enable && cfg.database.createLocally) {
    services.postgresql = {
      enable = true;
      package = pkgs.postgresql_18.withPackages (pp: [ pp.pgvector ]);
      ensureDatabases = [ cfg.database.name ];
      ensureUsers = [ { name = cfg.database.user; ensureDBOwnership = true; } ];
      settings = {
        random_page_cost = 1.1;
        timezone = "UTC";
        log_timezone = "UTC";
      };
    };
  };
}