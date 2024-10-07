{ self', pkgs, ... }:
{
  program = "${pkgs.writeScript "updateDeps" "${self'.packages.default.fetch-deps} nix/deps.nix"}";
}
