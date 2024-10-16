{
  lib,
  inputs,
  namespace,
  pkgs,
  stdenv,
  ...
}: let
  fs = pkgs.lib.fileset;

  root = ../../sources;
  pname = "PhoeNix.WebAPP";
  dotnet-sdk = dotnetCorePackages.sdk_8_0;

  csProjDepsHelper = lib."internal".csprojFileset {
    inherit dotnet-sdk;
    inherit root;
    project = pname;
    extraProjects = ["PhoeNix.WebAPI" "PhoeNix.WebAPP.Client"];
  };

  miscFiles = [
    ../../sources/.config
    ../../sources/PhoeNix.sln
    ../../sources/Directory.Build.props
  ];

  sourceFiles = fs.unions (csProjDepsHelper.projectPaths ++ miscFiles);

  inherit (pkgs) dotnetCorePackages buildDotnetModule;
in
  buildDotnetModule rec {
    inherit pname;
    version = "0";

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
