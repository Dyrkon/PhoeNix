{
    # Snowfall Lib provides a customized `lib` instance with access to your flake's library
    # as well as the libraries available from your flake's inputs.
    lib,
    # You also have access to your flake's inputs.
    inputs,
    # The namespace used for your flake, defaulting to "internal" if not set.
    namespace,

    # All other arguments come from NixPkgs. You can use `pkgs` to pull shells or helpers
    # programmatically or you may add the named attributes as arguments here.
    pkgs,
    mkShell,
    ...
}:
let
  solution = phoenix.overlays.solution;
  shell = mkShell {
    packages =
      solution.runtimeDeps
      ++ [ solution.dotnet-sdk solution.dotnet-runtime pkgs.nixos-anywhere ];
    shellHook = ''
      export DOTNET_ROOT=${solution.dotnet-runtime}
      export LD_LIBRARY_PATH="${solution.dotnet-sdk.icu}/lib:${pkgs.lib.makeLibraryPath solution.runtimeDeps}"
      unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
    '';
  };
in
shell
