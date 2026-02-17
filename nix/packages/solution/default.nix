{
  pkgs,
  lib,
  project,
  csprojSrc,
}:
pkgs.buildDotnetModule rec {
  pname = "phoenix-solution";
  version = builtins.readFile project.versionFile;

  src = csprojSrc;

  projectFile = "PhoeNix.sln";
  nugetDeps = project.nugetDeps;

  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;

  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = true;

  installPhase = ''mkdir -p $out'';

  dontFixup = true;
}
