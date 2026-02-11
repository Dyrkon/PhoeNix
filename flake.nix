{
  description = "PhoeNix flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-24.05";
    disko.url = "github:nix-community/disko/latest";
    disko.inputs.nixpkgs.follows = "nixpkgs";

    snowfall-lib = {
      url = "github:snowfallorg/lib";
      inputs.nixpkgs.follows = "nixpkgs";
    };

    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
  };

  outputs = {self, ...} @ inputs: let
    projectRoot = ./.;
    namespace = "phoenix";
    lib = inputs.snowfall-lib.mkLib {
      inherit inputs;
      src = projectRoot;
      snowfall.root = ./nix;
      namespace = namespace;
    };
  in
    lib.mkFlake {
      imports = [
        inputs.process-compose-flake.flakeModule
      ];

      outputs-builder = channels: {
        formatter = channels.nixpkgs.alejandra;

        apps = rec {
          updateDeps = import ./nix/apps/updateDeps.nix {
            inputs = inputs;
            pkgs = channels.nixpkgs;
          };

          playwright = import ./nix/apps/playwrightWithSettings.nix {
            inputs = inputs;
            pkgs = channels.nixpkgs;
          };
        };
      };
    };
}
