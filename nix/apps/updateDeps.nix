{
  inputs,
  pkgs,
  ...
}: {
  type = "app";
  program = "${pkgs.writeScript "updateDeps" "${inputs.self.packages.${pkgs.system}.solution.fetch-deps} deps.nix"}";
}
