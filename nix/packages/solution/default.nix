{  # Snowfall Lib provides a customized `lib` instance with access to your flake's library
  # as well as the libraries available from your flake's inputs.
  lib,
  # You also have access to your flake's inputs.
  inputs,
  # The namespace used for your flake, defaulting to "internal" if not set.
  namespace,
  # All other arguments come from NixPkgs. You can use `pkgs` to pull packages or helpers
  # programmatically or you may add the named attributes as arguments here.
  pkgs,
  stdenv,
  mkShell,
  ... 
}:
let
  fs = lib.fileset;

  root = ../..;

  sourceFiles = fs.unions [ (lib.path.append root "sources") ];

  PhoeNix = pkgs.buildDotnetModule rec {
    pname = "PhoeNix";
    version = lib.strings.fileContents (lib.path.append root "../version");

    src = fs.toSource {
      root = (lib.path.append root "sources");
      fileset = sourceFiles;
    };

    projectFile = "${pname}.sln"; # path to csproj or sln to build
    nugetDeps = ../../deps.nix;

    dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
    dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0;

    runtimeDeps = [ ];

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
        inputsFrom = [ PhoeNix ];
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
