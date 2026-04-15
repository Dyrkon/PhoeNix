{ self }:
{ config, lib, pkgs, ... }:
let
  cfg = config.services.phoenix;
in
{
  imports = [
    (import ../options.nix { inherit self lib pkgs; })
    ../api.nix
    ../database.nix
    ../monitoring.nix
    ../nginx.nix
  ];

  config = lib.mkIf cfg.enable {
    users.groups.${cfg.group} = { };
    users.users.${cfg.user} = {
      isSystemUser = true;
      group = cfg.group;
      home = cfg.stateDir;
    };
  };
}