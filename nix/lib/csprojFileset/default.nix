{
  pkgs,
  project,
}:
pkgs.lib.cleanSourceWith {
  src = project.sources;

  filter = path: type: let
    p = toString path;
    base = baseNameOf p;
  in
    !(pkgs.lib.any (needle: pkgs.lib.hasInfix needle p) [
      "/bin/"
      "/obj/"
      "/TestResults/"
    ])
    && base != "identifier.sqlite"
    && !pkgs.lib.hasSuffix ".DotSettings.user" p;
}
