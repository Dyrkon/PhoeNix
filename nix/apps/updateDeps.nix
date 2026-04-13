{ pkgs, packages, ... }:
{
  type = "app";
  meta.description = "Regenerate nix/deps.json and nix/webapp-deps.json";
  program = "${pkgs.writeShellScript "updateDeps" ''
    set -euo pipefail
    
    echo "Fetching backend dependencies (ignoring WASM)..."
    ${packages.solution.passthru.fetch-deps} nix/deps.json
    
    echo "Fetching frontend WASM dependencies..."
    exec ${packages.webapp.passthru.fetch-deps} nix/webapp-deps.json
  ''}";
}