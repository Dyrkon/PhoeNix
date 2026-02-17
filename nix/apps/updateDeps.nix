{ pkgs, packages, ... }:
{
  type = "app";
  meta.description = "Regenerate nix/deps.json for buildDotnetModule";
  program = "${pkgs.writeShellScript "updateDeps" ''
    set -euo pipefail
    exec ${packages.solution.passthru.fetch-deps} nix/deps.json
  ''}";
}
