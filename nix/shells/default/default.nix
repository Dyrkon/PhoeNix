{ pkgs, lib, project, qmd, runtimeDeps ? [] }:

let
  cudaPackages = pkgs.cudaPackages_12;
in
pkgs.mkShell {
  packages = [
    project.dotnetSdk
    project.dotnetRuntime
    pkgs.alejandra
    pkgs.nodejs
    pkgs.powershell
    pkgs.nixos-anywhere
    pkgs.process-compose
    pkgs.nginx
    pkgs.pixiecore
    pkgs.cmake
    pkgs.ninja
    cudaPackages.cuda_nvcc
    cudaPackages.cuda_cudart
    cudaPackages.libcublas
    qmd
    pkgs.claude-code
  ];

  shellHook = ''
    export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
    export DOTNET_ROOT=${project.dotnetSdk}

    export PATH="${cudaPackages.cuda_nvcc}/bin:$PATH"
    export CUDACXX="${cudaPackages.cuda_nvcc}/bin/nvcc"

    export LD_LIBRARY_PATH="/run/opengl-driver/lib:${project.dotnetSdk.icu}/lib:${cudaPackages.cuda_cudart}/lib:${cudaPackages.libcublas}/lib:${lib.makeLibraryPath runtimeDeps}''${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"

    unset GGML_CUDA
    unset CUDA_PATH
    unset CUDAToolkit_ROOT
    unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
  '';
}
