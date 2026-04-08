{
  pkgs,
  lib,
  qmd,
}: let
  cudaPackages = pkgs.cudaPackages_12;
in {
  type = "app";
  program = "${pkgs.writeShellScript "qmd-cuda-rebuild" ''
    set -euo pipefail

    QMD_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd-runtime"
    QMD_SHARED_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd"

    export LLAMA_CPP_LOCAL_BUILDS_DIR="$QMD_SHARED_CACHE/llama-builds"
    export PATH="${cudaPackages.cuda_nvcc}/bin:$PATH"
    export CUDACXX="${cudaPackages.cuda_nvcc}/bin/nvcc"
    export CUDA_PATH="${cudaPackages.cuda_nvcc}"
    export LD_LIBRARY_PATH="/run/opengl-driver/lib:${cudaPackages.cuda_cudart}/lib:${cudaPackages.libcublas}/lib''${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"

    echo "Rebuilding qmd CUDA backend..."
    echo "Shared build dir: $LLAMA_CPP_LOCAL_BUILDS_DIR"

    if find "$LLAMA_CPP_LOCAL_BUILDS_DIR" -type f | grep -q .; then
      echo "Existing qmd CUDA build found, skipping rebuild."
      exit 0
    fi
    mkdir -p "$LLAMA_CPP_LOCAL_BUILDS_DIR"

    # Make sure the runtime tree exists for the current packaged qmd.
    "${qmd}/bin/qmd" status >/dev/null || true

    cd "$QMD_CACHE"

    if [ ! -x "./node_modules/.bin/node-llama-cpp" ]; then
      echo "node-llama-cpp CLI not found in $QMD_CACHE" >&2
      exit 1
    fi

    exec ./node_modules/.bin/node-llama-cpp source download --gpu cuda
  ''}";
}