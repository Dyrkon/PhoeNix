{
  pkgs,
  lib,
  project,
  csprojSrc,
}: let
  pw = import ../../../lib/playwright/default.nix {inherit pkgs lib;};
in
  pkgs.buildDotnetModule rec {
    pname = "webapp-test";
    version = builtins.readFile project.versionFile;

    src = csprojSrc;
    projectFile = "tests/PhoeNix.WebAPP.Tests/PhoeNix.WebAPP.Tests.csproj";
    nugetDeps = project.nugetDeps;

    dotnet-sdk = project.dotnetSdk;
    dotnet-runtime = project.dotnetRuntime;

    buildType = "Release";
    useAppHost = false;
    selfContainedBuild = false;

    nativeBuildInputs = [pkgs.playwright-driver pkgs.nodejs];
    buildInputs = pw.runtimeLibs;

    installPhase = ''mkdir -p $out'';

    doCheck = true;
    checkPhase = ''
      runHook preCheck
      export HOME="$TMPDIR/home"
      mkdir -p "$HOME"

      ${pw.mkRunSettingsShell}

      dotnet test ${projectFile} \
        --configuration ${buildType} \
        --no-restore \
        --verbosity normal \
        --settings "$RUNSETTINGS"

      runHook postCheck
    '';

    dontFixup = true;
  }
