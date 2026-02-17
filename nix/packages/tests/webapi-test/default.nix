{
  pkgs,
  lib,
  project,
  csprojSrc,
}:
pkgs.buildDotnetModule rec {
  pname = "webapi-test";
  version = builtins.readFile project.versionFile;

  src = csprojSrc;

  projectFile = "tests/PhoeNix.WebAPI.Tests/PhoeNix.WebAPI.Tests.csproj";
  nugetDeps = project.nugetDeps;

  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;

  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = false;

  installPhase = ''mkdir -p $out'';

  doCheck = true;

  checkPhase = ''
    runHook preCheck
    export HOME="$TMPDIR/home"
    mkdir -p "$HOME"

    dotnet test ${projectFile} \
      --configuration ${buildType} \
      --no-restore \
      --verbosity normal


    runHook postCheck
  '';

  dontFixup = true;
}
