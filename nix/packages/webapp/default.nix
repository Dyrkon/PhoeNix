{ pkgs, lib, project, csprojSrc }:
pkgs.buildDotnetModule rec {
  pname = "PhoeNix.WebAPP";
  version = builtins.readFile project.versionFile;
  src = csprojSrc;
  runtimeId = "browser-wasm";
  projectFile = "src/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj";
  nugetDeps = project.webappDeps;
  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;
  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = true; 
  meta.mainProgram = pname;
}