{
  lib,
  inputs,
  namespace,
  pkgs,
  stdenv,
  mkShell,
  ...
}: let
  fs = lib.fileset;

  root = inputs.self.snowfall.config.src;

  sourceFiles = fs.unions [(lib.path.append root "sources")];

  PhoeNix = pkgs.buildDotnetModule rec {
    pname = "PhoeNix";
    version = builtins.readFile (lib.path.append root "version");

    src = fs.toSource {
      root = lib.path.append root "sources";
      fileset = sourceFiles;
    };

    projectFile = "${pname}.sln"; # path to csproj or sln to build
    nugetDeps = ../../deps.nix;

    dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
    dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0;

    runtimeDeps = [];

    useAppHost = false;

    buildPhase = ''
      dotnet build ${projectFile} -maxcpucount:1 -p:BuildInParallel=false -p:ContinuousIntegrationBuild=true -p:Deterministic=true --configuration Release --no-restore -p:InformationalVersion=${version} -p:Version=${version}.0
    '';

    installPhase = ''
      mkdir -p $out # ignores outputs
    '';

    dontDotnetCheck = true;
    dontFixup = true;

    passthru = {
      devShell = mkShell {
        inputsFrom = [PhoeNix];
        shellHook = ''
          export DOTNET_ROOT=${PhoeNix.dotnet-runtime}
          export LD_LIBRARY_PATH="${PhoeNix.dotnet-sdk.icu}/lib:${lib.makeLibraryPath PhoeNix.runtimeDeps}"
          unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
        '';
      };
    };
  };
in
  PhoeNix
