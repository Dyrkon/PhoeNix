{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-24.05";
    flake-parts.url = "github:hercules-ci/flake-parts";
    haumea.url = "github:nix-community/haumea/v0.2.2";
    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
    nuget-config.url = "github:nesfit/nixpkgs/fetchnuget-support-nuget.config";
    treefmt-nix.url = "github:numtide/treefmt-nix";
  };

  outputs =
    { self, ... }@inputs:
    let
      lib = inputs.nixpkgs.lib;
      h = inputs.haumea.lib;
      crossSystem = {
        config = "aarch64-unknown-linux-gnu";
      };
    in
    inputs.flake-parts.lib.mkFlake { inherit inputs; } (
      { config, ... }:
      {
        imports = [
          inputs.process-compose-flake.flakeModule
          inputs.treefmt-nix.flakeModule
        ];

        systems = [
          "x86_64-linux"
          "aarch64-linux"
        ];

        flake = {
          lib = h.load {
            src = ./nix/lib;
            inputs = {
              inherit lib;
            };
          };
        };

        perSystem =
          {
            self',
            inputs',
            pkgs,
            system,
            ...
          }:
          {
            apps = h.load {
              src = ./nix/apps;
              loader = h.loaders.default;
              inputs = { inherit self' inputs' pkgs; };
            };

            packages = h.load {
              src = ./nix/pkgs;
              inputs = {
                inherit self' inputs' pkgs;
                flib = self.lib;
              };
            } // (import ./nix/pxe-starters/pxe-starter.nix { 
                inherit self' inputs' pkgs; 
                nixpkgs' = inputs.nixpkgs;
                crossSystem = crossSystem;
              });

            devShells = h.load {
              src = ./nix/devShells;
              loader = h.loaders.default;
              inputs = { inherit self' inputs' pkgs; };
            };

            process-compose = h.load {
              src = ./nix/processes;
              loader = h.loaders.default;
              inputs = {
                inherit pkgs inputs' self';
              };
            };

            treefmt = {
              projectRootFile = "flake.nix";
              programs = {
                nixfmt-rfc-style.enable = true;
              };
              settings = {
                global.excludes = [ "nix/deps.nix" ];
              };
            };
          };
      }
    );
}
