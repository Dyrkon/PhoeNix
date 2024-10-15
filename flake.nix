{
  description = "PhoeNix flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-24.05";

    snowfall-lib = {
      url = "github:snowfallorg/lib";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = inputs:
    let
      lib = inputs.snowfall-lib.mkLib {
        # You must pass in both your flake's inputs and the root directory of
        # your flake.
        inherit inputs;
        src = ./.;

        # You can optionally place your Snowfall-related files in another
        # directory.
        snowfall.root = ./nix;
      };
    in
    # We'll cover what to do here next.
    lib.mkFlake {
      alias = {
          packages = {
            default = "solution";
          };
        };
    };
}