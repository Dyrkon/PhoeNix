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
  chrome-version = "1091";
  chrome-path = if stdenv.isLinux then 
    "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-linux/chrome" 
  else 
    "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-mac/Chromium.app/Contents/MacOS/Chromium";

  shell = mkShell {
    packages =
      solution.runtimeDeps
      ++ [
        solution.dotnet-sdk
        solution.dotnet-runtime
        pkgs.nixos-anywhere
        pkgs.alejandra
        pkgs.nodejs
        pkgs.playwright-driver.browsers];
    shellHook = ''
      export PLAYWRIGHT_NODEJS_PATH=${pkgs.nodejs}/bin/node;
      export PLAYWRIGHT_LAUNCH_OPTIONS_EXECUTABLE_PATH=${pkgs.playwright-driver.browsers}/chromium-1091/chrome-linux/chrome;
      export PLAYWRIGHT_BROWSERS_PATH=${pkgs.playwright-driver.browsers}
      export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
      export DOTNET_ROOT=${solution.dotnet-runtime}
      export LD_LIBRARY_PATH="${solution.dotnet-sdk.icu}/lib:${pkgs.lib.makeLibraryPath solution.runtimeDeps}"
      unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
    '';
  };
in
  shell
