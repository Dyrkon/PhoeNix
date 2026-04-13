{ pkgs, lib, qmd }:

let
  stdenv = pkgs.stdenv;
  isLinux = stdenv.isLinux;
  isDarwin = stdenv.isDarwin;
  cudaPackages = if isLinux then pkgs.cudaPackages_12 else null;
in {
  type = "app";
  program = "${pkgs.writeShellScript "qmd-rebuild" ''
    set -euo pipefail

    QMD_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd-runtime"
    QMD_SHARED_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd"

    export LLAMA_CPP_LOCAL_BUILDS_DIR="$QMD_SHARED_CACHE/llama-builds"

    if [ "$(uname)" = "Linux" ]; then
      export PATH="${cudaPackages.cuda_nvcc}/bin:$PATH"
      export CUDACXX="${cudaPackages.cuda_nvcc}/bin/nvcc"
      GPU_BACKEND="cuda"
    else
      GPU_BACKEND="metal"
    fi

    echo "Rebuilding qmd backend ($GPU_BACKEND)..."

    mkdir -p "$LLAMA_CPP_LOCAL_BUILDS_DIR"

    "${qmd}/bin/qmd" status >/dev/null || true

    cd "$QMD_CACHE"

    exec ./node_modules/.bin/node-llama-cpp source download --gpu "$GPU_BACKEND"
  ''}";
}