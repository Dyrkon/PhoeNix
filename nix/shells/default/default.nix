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
  chrome-path =
    if stdenv.isLinux
    then "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-linux/chrome"
    else "${pkgs.playwright-driver.browsers}/chromium-${chrome-version}/chrome-mac/Chromium.app/Contents/MacOS/Chromium";

  nodePath =
    if stdenv.isLinux
    then "sources/tests/PhoeNix.WebAPP.Tests/bin/Debug/net8.0/.playwright/node/linux-x64/node"
    else "sources/tests/PhoeNix.WebAPP.Tests/bin/Debug/net8.0/.playwright/node/mac/node";

  shell = mkShell {
    packages =
      solution.runtimeDeps
      ++ [
        solution.dotnet-sdk
        solution.dotnet-runtime
        pkgs.nixos-anywhere
        pkgs.alejandra
        pkgs.nodejs
        pkgs.playwright-driver.browsers
      ];
    shellHook = ''
      export PLAYWRIGHT_NODEJS_PATH=${pkgs.nodejs}/bin/node;
      export PLAYWRIGHT_LAUNCH_OPTIONS_EXECUTABLE_PATH=${chrome-path};
      export PLAYWRIGHT_BROWSERS_PATH=${pkgs.playwright-driver.browsers}
      export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
      export DOTNET_ROOT=${solution.dotnet-runtime}
      export LD_LIBRARY_PATH="${solution.dotnet-sdk.icu}/lib:${pkgs.lib.makeLibraryPath solution.runtimeDeps}"
      unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
      rm ./${nodePath}
      ln -s ${pkgs.nodejs}/bin/node ./${nodePath}
    '';
  };
in
  shell
