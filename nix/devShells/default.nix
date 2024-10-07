{ self'
, inputs'
, pkgs
, ...
}:
let
  solution = self'.packages.solution;
  shell = pkgs.mkShell {
    packages =
      solution.runtimeDeps
      ++ [ solution.dotnet-sdk solution.dotnet-runtime pkgs.nixos-anywhere self'.packages.pxe-starter ];
    shellHook = ''
      export DOTNET_ROOT=${solution.dotnet-runtime}
      export LD_LIBRARY_PATH="${solution.dotnet-sdk.icu}/lib:${pkgs.lib.makeLibraryPath solution.runtimeDeps}"
      unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
    '';
  };
in
shell
