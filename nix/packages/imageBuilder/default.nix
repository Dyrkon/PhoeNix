{pkgs, ...}:
pkgs.writeShellScriptBin "phoenix-create-pxe-image" ''
          if [ -z "''${PHOENIX_FLAKE_ROOT}" ]; then
            echo "phoenix-create-pxe-image: PHOENIX_FLAKE_ROOT is not set. Enter the nix dev shell first." >&2
            exit 1
          fi
          exec nix run "''${PHOENIX_FLAKE_ROOT}#bootstrap" --impure "$@"
        ''