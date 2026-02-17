{
  pkgs,
  lib,
  project,
}: let
  pw = import ../lib/playwright/default.nix {inherit pkgs lib;};
in {
  type = "app";
  program = "${pkgs.writeShellScript "playwright" ''
    set -euo pipefail
    ${pw.mkRunSettingsShell}

    exec ${project.dotnetSdk}/bin/dotnet test \
      ${project.sources}/PhoeNix.sln \
      --configuration Release \
      --settings "$RUNSETTINGS"
  ''}";
}
