{ pkgs, lib, project, qmd, runtimeDeps ? [], playwright, createPxeImage }:
let
  isLinux = pkgs.stdenv.isLinux;
  isDarwin = pkgs.stdenv.isDarwin;
  cudaPackages = pkgs.cudaPackages_12;
  
  isCI = (builtins.getEnv "CI") == "true";
in
pkgs.mkShell {
  packages =
    [
      project.dotnetSdk
      project.dotnetRuntime
      createPxeImage
    ]
    ++ lib.optionals (!isCI) [
      pkgs.alejandra
      pkgs.nodejs
      pkgs.powershell
      pkgs.nixos-anywhere
      pkgs.process-compose
      pkgs.nginx
      pkgs.pixiecore
      pkgs.cmake
      pkgs.ninja
      pkgs.claude-code
      qmd
      playwright.playwright-mcp
      playwright.playwright-dotnet
      playwright.playwright-cli
    ]
    ++ lib.optionals (isLinux && !isCI) [
      cudaPackages.cuda_nvcc
      cudaPackages.cuda_cudart
      cudaPackages.libcublas
    ]
    ++ lib.optionals (isDarwin && !isCI) [
      pkgs.apple-sdk_15
    ];

  shellHook = ''
    export DOTNET_ROOT=${project.dotnetSdk}
    export PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1
    export PLAYWRIGHT_NODEJS_PATH=${pkgs.nodejs}/bin/node
    export PLAYWRIGHT_BROWSERS_PATH=${playwright.playwright-dotnet-1_59_0-browsers}
    export PLAYWRIGHT_TEST_BASE_URL=http://localhost:5002

    ${lib.optionalString (isLinux && !isCI) ''
      export PATH="${cudaPackages.cuda_nvcc}/bin:$PATH"
      export CUDACXX="${cudaPackages.cuda_nvcc}/bin/nvcc"
      export LD_LIBRARY_PATH="${cudaPackages.cuda_cudart}/lib:${cudaPackages.libcublas}/lib''${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
    ''}

    # Unset variables only if they aren't needed
    ${lib.optionalString (!isCI) ''
      unset GGML_CUDA
      unset CUDA_PATH
    ''}
    unset DOTNET_SKIP_FIRST_TIME_EXPERIENCE
  '';
}