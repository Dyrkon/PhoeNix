{
  description = "PhoeNix flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-25.11";
    flake-utils.url = "github:numtide/flake-utils";

    disko.url = "github:nix-community/disko/latest";
    disko.inputs.nixpkgs.follows = "nixpkgs";

    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
  };

  outputs = inputs @ {
    self,
    nixpkgs,
    flake-utils,
    ...
  }:
    flake-utils.lib.eachDefaultSystem (
      system: let
        pkgs = import nixpkgs {inherit system;};
        lib = pkgs.lib;

        project = {
          root = ./.;
          sources = ./sources;
          versionFile = ./version;
          nugetDeps = ./nix/deps.json;
          dotnetSdk = pkgs.dotnetCorePackages.sdk_10_0;
          dotnetRuntime = pkgs.dotnetCorePackages.aspnetcore_10_0;
        };

        csprojSrc = import ./nix/lib/csprojFileset/default.nix {inherit pkgs project;};

        packages = rec {
          solution = import ./nix/packages/solution/default.nix {inherit pkgs lib project csprojSrc;};
          webapi = import ./nix/packages/webapi/default.nix {inherit pkgs lib project csprojSrc;};
          webapp = import ./nix/packages/webapp/default.nix {inherit pkgs lib project csprojSrc;};

          pxe-starter = import ./nix/packages/pxe-starter/default.nix {inherit pkgs lib inputs project;};

          default = webapp;
        };

        webapiTest = import ./nix/packages/tests/webapi-test/default.nix {inherit pkgs lib project csprojSrc;};
        webappTest = import ./nix/packages/tests/webapp-test/default.nix {inherit pkgs lib project csprojSrc;};

        devShell = import ./nix/shells/default/default.nix {
          inherit pkgs lib project;
          runtimeDeps = [];
        };

        pc = import ./nix/process-compose/default.nix {inherit pkgs lib project;};
      in {
        formatter = pkgs.alejandra;

        packages =
          packages
          // {
            process-compose-config = pc.configPackage;
          };

        devShells.default = devShell;

        apps = {
          updateDeps = import ./nix/apps/updateDeps.nix {
            inherit pkgs lib project;
            packages = packages;
          };

          playwright = import ./nix/apps/playwrightWithSettings.nix {
            inherit pkgs lib project;
          };

          up = {
            type = "app";
            program = "${pkgs.writeShellScript "phoenix-up" ''
              set -euo pipefail
              exec ${pkgs.process-compose}/bin/process-compose \
                -f ${pc.configPackage}/process-compose.yaml up
            ''}";
          };

          down = {
            type = "app";
            program = "${pkgs.writeShellScript "phoenix-down" ''
              set -euo pipefail
              exec ${pkgs.process-compose}/bin/process-compose \
                -f ${pc.configPackage}/process-compose.yaml down
            ''}";
          };
        };

        checks = {
          build-webapi = packages.webapi;
          build-webapp = packages.webapp;
          webapi-test = webapiTest;
          webapp-test = webappTest;
        };
      }
    );
}
