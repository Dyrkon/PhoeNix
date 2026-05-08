{
  description = "PhoeNix flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-25.11";
    flake-utils.url = "github:numtide/flake-utils";

    disko.url = "github:nix-community/disko/v1.10.0";
    disko.inputs.nixpkgs.follows = "nixpkgs";

    process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";

    qmd-src = {
      url = "github:tobi/qmd/c2f3a4037204066455662492518384c9f3a4d247";
      flake = false;
    };

    bun2nix.url = "github:nix-community/bun2nix";

    playwright.url = "github:halfwhey/nix-playwright-nightly";
  };

  outputs = inputs @ {
    self, nixpkgs, flake-utils, disko, playwright, ...
  }:
  let
      supportedSystems = [ "x86_64-linux" "aarch64-linux" "aarch64-darwin" ];
      
      nixosModules.default = import ./nix/modules/phoenix/default.nix { inherit self; };

      mkSystem = system: nixpkgs.lib.nixosSystem {
        inherit system;
        specialArgs = { inherit inputs; };
        modules = [
          disko.nixosModules.disko
          ./nix/configurations/phoenix-server/disko.nix
          ./nix/configurations/phoenix-server/default.nix
          { nixpkgs.hostPlatform = system; }
        ];
      };
      mkLxc = system: nixpkgs.lib.nixosSystem {
        inherit system;
        specialArgs = { inherit inputs; };
        modules = [
          "${nixpkgs}/nixos/modules/virtualisation/proxmox-lxc.nix"
          ./nix/configurations/phoenix-server/default.nix
        ];
      };
    in
    {
      inherit nixosModules;

      nixosConfigurations = {
        phoenix-x86 = mkSystem "x86_64-linux";
        phoenix-arm = mkSystem "aarch64-linux";
        phoenix-x86-lxc = mkLxc "x86_64-linux";
      };
    } //
    flake-utils.lib.eachDefaultSystem (
      system: let
        pkgs = import nixpkgs {
          inherit system;
          config = {
            allowUnfree = true;
            cudaSupport = pkgs.stdenv.isLinux;
          };
        };

        lib = pkgs.lib;

        project = {
          root = ./.;
          sources = ./sources;
          versionFile = ./version;
          nugetDeps = ./nix/deps.json;
          webappDeps = ./nix/webapp-deps.json;
          dotnetSdk = pkgs.dotnetCorePackages.sdk_10_0-bin;
          dotnetRuntime = pkgs.dotnetCorePackages.aspnetcore_10_0-bin;
        };

        csprojSrc = import ./nix/lib/csprojFileset/default.nix {
          inherit pkgs project;
        };

        packages = rec {
          solution = import ./nix/packages/solution/default.nix {
            inherit pkgs lib project csprojSrc;
          };

          webapi = import ./nix/packages/webapi/default.nix {
            inherit pkgs lib project csprojSrc;
          };

          mcpserver = import ./nix/packages/mcpserver/default.nix {
            inherit pkgs lib project csprojSrc;
          };

          webapp = import ./nix/packages/webapp/default.nix {
            inherit pkgs lib project csprojSrc;
          };

          qmd = import ./nix/packages/qmd/default.nix {
            inherit pkgs lib;
            qmd-src = inputs.qmd-src;
            bun2nix = inputs.bun2nix.packages.${system}.default;
          };

          default = webapp;

          lxc = self.nixosConfigurations."phoenix-${if system == "x86_64-linux" then "x86" else "arm"}".config.system.build.images.lxc;
          
          vm = self.nixosConfigurations."phoenix-${if system == "x86_64-linux" then "x86" else "arm"}".config.system.build.images.qemu;
        };

        webapiTest = import ./nix/packages/tests/webapi-test/default.nix {
          inherit pkgs lib project csprojSrc;
        };

        webappTest = import ./nix/packages/tests/webapp-test/default.nix {
          inherit pkgs lib project csprojSrc;
        };

        createPxeImageDev = import ./nix/packages/imageBuilder/default.nix  {
          inherit pkgs;
        };

        devShell = import ./nix/shells/default/default.nix {
          inherit pkgs lib project;
          qmd = packages.qmd;
          runtimeDeps = [];
          playwright = playwright.packages.${system};
          createPxeImage = createPxeImageDev;
        };

        pc = import ./nix/process-compose/default.nix {
          inherit pkgs lib project;
        };
      in {
        formatter = pkgs.alejandra;

        packages = packages // {
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

          bootstrap = import ./nix/apps/createPxeImage.nix {
            inherit pkgs lib inputs project;
          };

          createIngest = import ./nix/apps/createIngest.nix {inherit pkgs;};
          
          qm-rebuild = import ./nix/apps/qmdCudaRebuild.nix {inherit pkgs lib; qmd = packages.qmd; };

          up = {
            type = "app";
            program = "${pkgs.writeShellScript "phoenix-up" ''
              set -euo pipefail
              exec ${pkgs.process-compose}/bin/process-compose \
                -p 8081 \
                -f ${pc.configPackage}/process-compose.yaml up
            ''}";
          };

          down = {
            type = "app";
            program = "${pkgs.writeShellScript "phoenix-down" ''
              set -euo pipefail
              exec ${pkgs.process-compose}/bin/process-compose \
                -p 8081 \
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