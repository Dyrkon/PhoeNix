{ pkgs, lib, project, qmd, runtimeDeps ? [] }:
let
  isLinux = pkgs.stdenv.isLinux;
  cudaPackages = pkgs.cudaPackages_12;
in
pkgs.mkShell {
  packages =
    [
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
      qmd
      pkgs.claude-code
    ]
    ++ lib.optionals isLinux [
      cudaPackages.cuda_nvcc
      cudaPackages.cuda_cudart
      cudaPackages.libcublas
    ];

  shellHook = ''
    export PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS=true
    export DOTNET_ROOT=${project.dotnetSdk}

    ${lib.optionalString isLinux ''
    export PATH="${cudaPackages.cuda_nvcc}/bin:$PATH"
    export CUDACXX="${cudaPackages.cuda_nvcc}/bin/nvcc"
    export LD_LIBRARY_PATH="${cudaPackages.cuda_cudart}/lib:${cudaPackages.libcublas}/lib''${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
    ''}

    unset GGML_CUDA
    unset CUDA_PATH
    unset CUDAToolkit_ROOT
    unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
  '';
}