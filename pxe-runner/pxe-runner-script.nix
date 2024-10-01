{ pkgs }:
let
  pixiecore = "${pkgs.pixiecore}/bin/pixiecore";
in
pkgs.writeShellScriptBin "" ''
  usage() {
    echo "Usage: $0 --flake <file path> --configuration <config_name>"
    exit 1
  }

  # Check if the number of arguments is correct
  if [ "$#" -lt 4 ]; then
      usage
  fi

  # Initialize variables
  flake_file=""
  config_name=""

  # Parse arguments
  while [[ "$#" -gt 0 ]]; do
      case $1 in
          --flake)
              flake_file="$2"
              shift 2
              ;;
          --configuration)
              config_name="$2"
              shift 2
              ;;
          *)
              echo "Unknown parameter passed: $1"
              usage
              ;;
      esac
  done

  # Verify if the flake file exists
  if [ -n "$flake_file" ]; then
      if [ -f "$flake_file" ]; then
          echo "File '$flake_file' exists."
      else
          echo "File '$flake_file' does not exist."
          exit 1
      fi
  fi

  # Read the configuration name
  if [ -n "$config_name" ]; then
      echo "Configuration name: $config_name"
  fi

  nixos-rebuild build --flake $flake_file#$config_name

  result_path=$(cd result/ ; pwd -P ; cd ..)

  ${pixiecore} boot $(realpath $result_path/kernel) $result_path/initrd \
    --cmdline "init=$result_path/init loglevel=4" \
    --debug --dhcp-no-bind \
    --port 64172 --status-port 64172 "$@"
''