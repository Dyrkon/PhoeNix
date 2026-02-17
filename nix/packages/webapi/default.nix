{
  pkgs,
  lib,
  project,
  csprojSrc,
}:
pkgs.buildDotnetModule rec {
  pname = "PhoeNix.WebAPI";
  version = builtins.readFile project.versionFile;

  src = csprojSrc;

  projectFile = "src/PhoeNix.WebAPI/PhoeNix.WebAPI.csproj";
  nugetDeps = project.nugetDeps;

  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;

  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = true;

  meta.mainProgram = pname;
}
