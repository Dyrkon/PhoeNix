{
  lib,
  inputs,
  namespace,
  pkgs,
  stdenv,
  ...
}: let
  fs = lib.fileset;

  flake-root = inputs.self.snowfall.config.src;
  root = lib.path.append flake-root "sources";
  pname = "PhoeNix.WebAPI";
  dotnet-sdk = dotnetCorePackages.sdk_8_0;

  csProjDepsHelper = lib."internal".csprojFileset {
    inherit dotnet-sdk;
    inherit root;
    project = pname;
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

    projectFile = "./src/${pname}/${pname}.csproj"; # path to csproj or sln to build
    nugetDeps = ../../deps.nix;

    inherit dotnet-sdk;
    dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

    runtimeDeps = [
      # add any native deps here
    ];

    useAppHost = false;

    postFixup = ''
      wrapProgram $out/bin/${meta.mainProgram} --add-flags "--defaultConfigurationPath=$out/lib/${pname}/appsettings.json"
    '';

    meta.mainProgram = pname;
  }
