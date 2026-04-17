{ pkgs, lib, project, csprojSrc }:
let
  resolveFingerprints = pkgs.writeScript "resolve-fingerprints.py" ''
import json, sys, os

with open(sys.argv[1]) as f:
    data = json.load(f)

wwwroot = os.path.dirname(sys.argv[2])
replacements = {}
import_map = {}

for ep in data.get("Endpoints", []):
    props = {p["Name"]: p["Value"] for p in (ep.get("EndpointProperties") or [])}
    selectors = {s["Name"]: s["Value"] for s in (ep.get("Selectors") or [])}

    if "Content-Encoding" in selectors:
        continue

    if "fingerprint" not in props or "label" not in props:
        continue

    label = props["label"]
    route = ep["Route"]

    if label not in replacements:
        parts = label.rsplit(".", 1)
        if len(parts) == 2:
            key = parts[0] + "#[.{fingerprint}]." + parts[1]
            replacements[key] = route

    if route.startswith("_framework/") and route.endswith(".js") and label != route:
        import_map["./_framework/" + label] = "./_framework/" + route.split("/", 1)[1]

with open(sys.argv[2]) as f:
    html = f.read()

for placeholder, route in replacements.items():
    html = html.replace(placeholder, route)

if import_map:
    import_map_json = json.dumps({"imports": import_map}, indent=2)
    html = html.replace(
        '<script type="importmap"></script>',
        '<script type="importmap">' + import_map_json + '</script>'
    )

with open(sys.argv[2], "w") as f:
    f.write(html)

print(f"Fingerprint placeholders resolved: {len(replacements)}")
print(f"Import map entries: {len(import_map)}")
'';
in
pkgs.buildDotnetModule rec {
  pname = "PhoeNix.WebAPP";
  version = builtins.readFile project.versionFile;
  src = csprojSrc;
  runtimeId = "browser-wasm";
  projectFile = "src/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj";
  nugetDeps = project.webappDeps;
  dotnet-sdk = project.dotnetSdk;
  dotnet-runtime = project.dotnetRuntime;
  buildType = "Release";
  useAppHost = false;
  selfContainedBuild = true;

  postFixup = ''
    local wwwroot="$out/lib/${pname}/wwwroot"
    local index="$wwwroot/index.html"
    local endpoints="$out/lib/${pname}/${pname}.staticwebassets.endpoints.json"

    if [ -f "$endpoints" ] && [ -f "$index" ]; then
      chmod +w "$index"
      ${pkgs.python3}/bin/python3 ${resolveFingerprints} "$endpoints" "$index"
    fi

    chmod +w "$wwwroot/_framework"
    for f in "$wwwroot"/_framework/*.js; do
      [ -f "$f" ] || continue
      name="$(basename "$f")"
      bare="$(echo "$name" | sed -E 's/\.[a-z0-9]{8,12}\.js$/.js/')"
      if [ "$bare" != "$name" ] && [ ! -e "$wwwroot/_framework/$bare" ]; then
        ln -s "$name" "$wwwroot/_framework/$bare"
      fi
    done

    mv "$wwwroot"/* "$out/"
    rm -rf "$out/bin" "$out/lib"
  '';

  meta.mainProgram = pname;
}