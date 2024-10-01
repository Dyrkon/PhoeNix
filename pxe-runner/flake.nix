{
  description = "A very basic flake";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixos-unstable";
  };

  outputs = { self, nixpkgs }: 
  let
    pkgs = nixpkgs.legacyPackages.x86_64-linux;
  in
  {
    devShell = pkgs.mkShell {
        buildInputs = [
          import ./pxe-runner-script.nix {inherit pkgs;}
        ];
    };
  };
}
