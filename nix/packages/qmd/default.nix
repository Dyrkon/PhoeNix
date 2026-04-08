{ pkgs, lib, qmd-src, bun2nix }:

let
  cudaPackages = pkgs.cudaPackages_12;

  patchedSrc = pkgs.applyPatches {
    name = "qmd-src";
    src = qmd-src;
    patches = [
      ../../patches/qmd-llm-cuda.patch
    ];
  };
in
bun2nix.mkDerivation {
  pname = "qmd";
  version = "2.0.1";
  src = patchedSrc;

  bunDeps = bun2nix.fetchBunDeps {
    pname = "qmd-deps";
    version = "2.0.1";
    src = patchedSrc;
    bunNix = ./bun.nix;
  };

  nativeBuildInputs = [
    pkgs.makeWrapper
    pkgs.python3
    cudaPackages.cuda_nvcc
  ];

  buildInputs = [
    pkgs.sqlite
    cudaPackages.cuda_cudart
    cudaPackages.libcublas
  ];

  dontUseCmakeConfigure = true;
  dontConfigure = true;

  buildPhase = ''
    true
  '';

  installPhase = ''
    mkdir -p "$out/lib/qmd" "$out/bin"

    cp -r node_modules "$out/lib/qmd/"
    cp -r src "$out/lib/qmd/"
    cp package.json "$out/lib/qmd/"

    if [ -f tsconfig.json ]; then
      cp tsconfig.json "$out/lib/qmd/"
    fi

    if [ -d dist ]; then
      cp -r dist "$out/lib/qmd/"
    fi

    chmod -R u+w "$out/lib/qmd"

    find "$out/lib/qmd/node_modules" -type f -name 'detectGlibc.js' -print0 | \
      while IFS= read -r -d "" file; do
        substituteInPlace "$file" \
          --replace 'return false' 'return true'
      done

    cat > "$out/bin/qmd" << 'WRAPPER'
#!/bin/sh
set -eu

QMD_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd-runtime"
QMD_SHARED_CACHE="''${XDG_CACHE_HOME:-$HOME/.cache}/qmd"
QMD_STAMP="@out@"
QMD_STAMP_FILE="$QMD_CACHE/.qmd-package-stamp"

mkdir -p "$QMD_SHARED_CACHE/llama-builds"

if [ ! -f "$QMD_STAMP_FILE" ] || [ "$(cat "$QMD_STAMP_FILE")" != "$QMD_STAMP" ]; then
  rm -rf "$QMD_CACHE"
  mkdir -p "$QMD_CACHE"

  cp -r @out@/lib/qmd/node_modules "$QMD_CACHE/"
  cp -r @out@/lib/qmd/src "$QMD_CACHE/"
  cp @out@/lib/qmd/package.json "$QMD_CACHE/"

  if [ -f @out@/lib/qmd/tsconfig.json ]; then
    cp @out@/lib/qmd/tsconfig.json "$QMD_CACHE/"
  fi

  if [ -d @out@/lib/qmd/dist ]; then
    cp -r @out@/lib/qmd/dist "$QMD_CACHE/"
  fi

  chmod -R u+w "$QMD_CACHE"
  printf '%s\n' "$QMD_STAMP" > "$QMD_STAMP_FILE"
fi

export PATH="@nvcc_bin@:$PATH"
export CUDACXX="@nvcc_bin@/nvcc"
export LD_LIBRARY_PATH="/run/opengl-driver/lib:@sqlite_lib@:@cuda_lib@''${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export LLAMA_CPP_LOCAL_BUILDS_DIR="$QMD_SHARED_CACHE/llama-builds"

unset GGML_CUDA
unset CUDA_PATH
unset CUDAToolkit_ROOT

cd "$QMD_CACHE"

if [ -f "$QMD_CACHE/dist/qmd.js" ]; then
  exec @bun@/bin/bun "$QMD_CACHE/dist/qmd.js" "$@"
else
  exec @bun@/bin/bun "$QMD_CACHE/src/cli/qmd.ts" "$@"
fi
WRAPPER

    substituteInPlace "$out/bin/qmd" \
      --replace @out@ "$out" \
      --replace @bun@ ${pkgs.bun} \
      --replace @nvcc_bin@ ${cudaPackages.cuda_nvcc}/bin \
      --replace @sqlite_lib@ ${pkgs.sqlite.out}/lib \
      --replace @cuda_lib@ "${cudaPackages.cuda_cudart}/lib:${cudaPackages.libcublas}/lib"

    chmod +x "$out/bin/qmd"
  '';

  meta = {
    description = "On-device hybrid search for markdown docs";
    homepage = "https://github.com/tobi/qmd";
    license = lib.licenses.mit;
    platforms = lib.platforms.unix;
    mainProgram = "qmd";
  };
}