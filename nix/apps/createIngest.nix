{ pkgs, ... }: 
let
    filter = "*.lock sources/src/PhoeNix.Persistence/Migrations/* *.json bun.nix";
    outputPath = "docs/digest.txt";
in {
    type = "app";
    meta.description = "Create filtered ingest from the repository.";
    program = "${pkgs.writeShellScript "createIngest" ''${pkgs.gitingest}/bin/gitingest . -e "${filter}" -o ${outputPath}''}";
}