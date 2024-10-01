{ pkgs }:
let
  pixiecore = "${pkgs.pixiecore}/bin/pixiecore";
in
pkgs.writeShellScriptBin "" ''
  ${pixiecore} boot ${pkgs.kernel}/bzImage ${pkgs.netbootRamdisk}/initrd \
    --cmdline "init=${pkgs.toplevel}/init loglevel=4" \
    --debug --dhcp-no-bind \
    --port 64172 --status-port 64172 "$@"
''