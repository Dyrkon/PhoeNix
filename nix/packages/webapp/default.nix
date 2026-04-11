{
  pkgs,
  lib,
  project,
  csprojSrc,
}:
pkgs.buildDotnetModule rec {
  pname = "PhoeNix.WebAPP";
  version = builtins.readFile project.versionFile;

  src = csprojSrc;

  # Fix: https://github.com/dotnet/maui/issues/32968
  dotnetRestoreFlags = [
    "-p:UseMonoRuntime=false"
  ];
  dotnetFlags = [
    "-p:UseMonoRuntime=false"
  ];

  projectFile = "src/PhoeNix.WebAPP/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj";
  nugetDeps = project.nugetDeps;

  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;

  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = false; # true causes error with linux runtime

  meta.mainProgram = pname;
}
