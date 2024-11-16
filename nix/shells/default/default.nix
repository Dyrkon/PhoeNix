{
  lib,
  inputs,
  namespace,
  pkgs,
  mkShell,
  ...
}: let
  inherit (pkgs) stdenv;
  solution = pkgs.${namespace}.solution;

  shell = mkShell {
    packages =
      solution.runtimeDeps
      ++ [
        solution.dotnet-sdk
        solution.dotnet-runtime
        pkgs.nixos-anywhere
        pkgs.alejandra
        pkgs.nodejs
        pkgs.powershell
      ];
    shellHook = ''
      export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
      export DOTNET_ROOT=${solution.dotnet-runtime}
      export LD_LIBRARY_PATH="${solution.dotnet-sdk.icu}/lib:${pkgs.lib.makeLibraryPath solution.runtimeDeps}"
      unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
    '';
  };
in
  shell
