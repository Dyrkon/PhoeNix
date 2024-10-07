{ self', pkgs, ... }:
with pkgs;
let
  lib = pkgs.lib;
  fs = lib.fileset;

  root = ../..;

  sourceFiles = fs.unions [ (lib.path.append root "sources") ];

  PhoeNix = buildDotnetModule rec {
    pname = "PhoeNix";
    version = lib.strings.fileContents (lib.path.append root "version");

    src = fs.toSource {
      root = (lib.path.append root "sources");
      fileset = sourceFiles;
    };

    projectFile = "${pname}.sln"; # path to csproj or sln to build
    nugetDeps = ../deps.nix;

    dotnet-sdk = dotnetCorePackages.sdk_8_0;
    dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

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
