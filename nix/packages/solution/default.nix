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
  selfContainedBuild = false; # true causes error with linux runtime

  # Fix: https://github.com/dotnet/maui/issues/32968
  dotnetRestoreFlags = [
    "-p:UseMonoRuntime=false"
  ];
  dotnetFlags = [
    "-p:UseMonoRuntime=false"
  ];

  installPhase = ''mkdir -p $out'';

  dontFixup = true;
}
