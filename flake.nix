{
  description = "PhoeNix flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-24.05";

    snowfall-lib = {
      url = "github:snowfallorg/lib";
      inputs.nixpkgs.follows = "nixpkgs";
    };

    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
  };

  outputs = {self, ...} @ inputs: let
    lib = inputs.snowfall-lib.mkLib {
      inherit inputs;
      src = ./.;
      snowfall.root = ./nix;
    };
  in
    lib.mkFlake {
      outputs-builder = channels: {
        formatter = channels.nixpkgs.alejandra;

        apps = rec {
          updateDeps = import ./nix/apps/updateDeps.nix {
            inputs = inputs;
            pkgs = channels.nixpkgs;
          };
        };
      };
    };
}
