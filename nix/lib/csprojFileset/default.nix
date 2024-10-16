{
  lib,
  inputs,
  namespace,
  snowfall-inputs,
}: {
  csprojFileset = {
    root,
    project,
    dotnet-sdk,
    extraProjects ? [],
    dotnetSdkVersion ? dotnet-sdk.version,
  }: let
    dotnetVersion = "net${lib.versions.majorMinor dotnetSdkVersion}";

    files = lib.filesystem.listFilesRecursive root;

    lockFile = builtins.head (
      builtins.filter (p: builtins.match ".*/${project}/packages.lock.json$" (toString p) != null) files
    );
    projectsLock = builtins.fromJSON (lib.strings.fileContents lockFile);
    deps = projectsLock.dependencies.${dotnetVersion};
    projectDeps = lib.attrNames (lib.attrsets.filterAttrs (n: v: v.type == "Project") deps);

    csProjs = builtins.filter (p: builtins.match ".*.csproj$" (toString p) != null) files;
    projectRoots = builtins.map (p: dirOf p) csProjs;

    getProjectDir = projectName:
      builtins.filter (
        p: builtins.match (lib.toLower projectName) (lib.toLower (toString (baseNameOf p))) != null
      )
      projectRoots;

    projectPaths = builtins.concatMap getProjectDir ([project] ++ projectDeps ++ extraProjects);
  in {
    inherit projectPaths;
  };
}
