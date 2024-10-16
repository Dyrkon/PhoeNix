{
  lib,
  inputs,
  namespace,
  pkgs,
  stdenv,
  ...
}: let
  fs = pkgs.lib.fileset;

  flake-root = inputs.self.snowfall.config.src;
  root = lib.path.append flake-root "sources";
  pname = "PhoeNix.WebAPP";
  dotnet-sdk = dotnetCorePackages.sdk_8_0;

  csProjDepsHelper = lib."internal".csprojFileset {
    inherit dotnet-sdk;
    inherit root;
    project = pname;
    extraProjects = ["PhoeNix.WebAPI" "PhoeNix.WebAPP.Client"];
  };

  miscFiles = map (i: lib.path.append root i) [
    ".config"
    "PhoeNix.sln"
    "Directory.Build.props"
  ];

  sourceFiles = fs.unions (csProjDepsHelper.projectPaths ++ miscFiles);

  inherit (pkgs) dotnetCorePackages buildDotnetModule;
in
  buildDotnetModule rec {
    inherit pname;
    version = builtins.readFile (lib.path.append flake-root "version");

    src = fs.toSource {
      inherit root;
      fileset = sourceFiles;
    };

    projectFile = "./src/${pname}/${pname}/${pname}.csproj"; # path to csproj or sln to build
    nugetDeps = ../../deps.nix;
    buildType = "Release";

    inherit dotnet-sdk;
    dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

    runtimeDeps = [
      # add any native deps here
    ];

    useAppHost = false;
    selfContainedBuild = true;

    makeWrapperArgs = [
      "--set DOTNET_CONTENTROOT ${placeholder "out"}/lib/${pname}"
    ];

    meta.mainProgram = pname;
  }
