{
  inputs,
  pkgs,
  ...
}: {
  type = "app";
  program = "${pkgs.writeScript "updateDeps" ''
    #!${pkgs.bash}/bin/bash
    ${inputs.self.packages.${pkgs.system}.solution.fetch-deps} deps.nix
  ''}";
}
