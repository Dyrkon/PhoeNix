{
  pkgs,
  lib,
  project,
  runtimeDeps ? [],
}:
pkgs.mkShell {
  packages = [
    project.dotnetSdk
    project.dotnetRuntime

    pkgs.alejandra
    pkgs.nodejs
    pkgs.powershell
    pkgs.nixos-anywhere
    pkgs.process-compose
    pkgs.nginx
  ];

  shellHook = ''
    export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
    export DOTNET_ROOT=${project.dotnetSdk}
    export LD_LIBRARY_PATH="${project.dotnetSdk.icu}/lib:${lib.makeLibraryPath runtimeDeps}"
    unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
  '';
}
