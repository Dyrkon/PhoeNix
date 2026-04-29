{
  pkgs,
  lib,
  inputs,
  project,
}:
let
  phoenixUserCaPublicKey =
    let
      value = builtins.getEnv "PHOENIX_USER_CA_PUBLIC_KEY";
    in
    if value == ""
    then throw "PHOENIX_USER_CA_PUBLIC_KEY environment variable is not set."
    else value;

  targetSystem =
    let
      value = builtins.getEnv "PHOENIX_TARGET_SYSTEM";
    in
    if value == ""
    then pkgs.stdenv.hostPlatform.system
    else value;

  phoenixBootstrapCallbackScript = pkgs.writeShellScript "phoenix-bootstrap-callback" ''
    set -euo pipefail

    cmdline="$(< /proc/cmdline)"

    get_arg() {
      local key="$1"
      for part in $cmdline; do
        case "$part" in
          "$key"=*)
            printf '%s\n' "''${part#"$key"=}"
            return 0
            ;;
        esac
      done
      return 1
    }

    api_base="$(get_arg phoenix.api-base || true)"
    session_id="$(get_arg phoenix.session-id || true)"
    machine_id="$(get_arg phoenix.machine-id || true)"
    callback_token="$(get_arg phoenix.callback-token || true)"

    echo "phoenix-bootstrap-callback: api_base=''${api_base:-<missing>} session_id=''${session_id:-<missing>} machine_id=''${machine_id:-<missing>}"

    if [ -z "''${api_base:-}" ] || [ -z "''${session_id:-}" ] || [ -z "''${machine_id:-}" ] || [ -z "''${callback_token:-}" ]; then
      echo "Missing required phoenix.* kernel parameters" >&2
      exit 1
    fi

    payload="$(cat <<EOF
{"sessionId":"$session_id","machineId":"$machine_id"}
EOF
)"

    exec ${pkgs.curl}/bin/curl \
      --fail \
      --silent \
      --show-error \
      --retry 10 \
      --retry-all-errors \
      --retry-delay 3 \
      --connect-timeout 5 \
      --max-time 20 \
      -X POST \
      -H "Authorization: Bearer $callback_token" \
      -H "Content-Type: application/json" \
      -d "$payload" \
      "$api_base/setup/bootstrap/callback"
  '';

  systemConfiguration = inputs.nixpkgs.lib.nixosSystem {
    system = targetSystem;
    modules = [
      ({ config, modulesPath, ... }: {
        imports = [
          (modulesPath + "/installer/netboot/netboot-minimal.nix")
        ];

        config = {
          services.openssh.enable = true;

          services.openssh.settings = {
            TrustedUserCAKeys = "/etc/ssh/phoenix_user_ca.pub";
            PermitRootLogin = "prohibit-password";
            PasswordAuthentication = false;
            KbdInteractiveAuthentication = false;
            PubkeyAuthentication = true;

            X11Forwarding = false;
            PermitTunnel = false;
            AllowAgentForwarding = false;
            AllowTcpForwarding = true;
          };

          environment.etc."ssh/phoenix_user_ca.pub".text = phoenixUserCaPublicKey;
          environment.etc."ssh/authorized_principals/root".text = ''root'';

          users.users.root.openssh.authorizedKeys.keys = lib.mkForce [ ];

          environment.systemPackages = [ pkgs.curl pkgs.nixos-facter ];

          nix.settings = {
            experimental-features = [ "nix-command" ];
          };

          systemd.services.phoenix-bootstrap-callback = {
            description = "Notify PhoeNix that bootstrap environment is ready";

            wantedBy = [ "multi-user.target" ];
            after = [ "network-online.target" "nss-lookup.target" ];
            wants = [ "network-online.target" "nss-lookup.target" ];

            unitConfig = {
              ConditionKernelCommandLine = [
                "phoenix.api-base"
                "phoenix.session-id"
                "phoenix.machine-id"
                "phoenix.callback-token"
              ];
            };

            serviceConfig = {
              Type = "oneshot";
              ExecStart = "${phoenixBootstrapCallbackScript}";
            };
          };

          system.stateVersion = config.system.nixos.release;
        };
      })
    ];
  };

  build = systemConfiguration.config.system.build;

  json = builtins.toJSON {
    kernel = "${build.kernel}/bzImage";
    ramDisk = "${build.netbootRamdisk}/initrd";
    init = "${build.toplevel}/init";
    system = targetSystem;
    kernelParams = "init=${build.toplevel}/init loglevel=7 console=ttyS0,115200n8";
  };
in
{
  type = "app";
  meta.description = "Create PXE bootstrap image with SSH CA trust";
  meta.platforms = ["x86_64-linux" "aarch64-linux"];
  program = "${pkgs.writeShellScript "create-pxe-image" ''
    set -euo pipefail
    echo '${json}'
  ''}";
}
