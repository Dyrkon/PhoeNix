{
  # Snowfall Lib provides a customized `lib` instance with access to your flake's library
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
  ...
}:
let
  fs = lib.fileset;

  root = ../../sources;
  pname = "PhoeNix.WebAPI";
  dotnet-sdk = dotnetCorePackages.sdk_8_0;

  csProjDeps = lib.csprojFileset {
    inherit dotnet-sdk;
    inherit root;
    project = pname;
  };

  miscFiles = [
    ../../sources/.config
    ../../sources/PhoeNix.sln
    ../../sources/Directory.Build.props
  ];

  sourceFiles = fs.unions (csProjDeps ++ miscFiles);

  inherit (pkgs) dotnetCorePackages buildDotnetModule;
in
buildDotnetModule rec {
  inherit pname;
  version = lib.strings.fileContents ../../version;

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
