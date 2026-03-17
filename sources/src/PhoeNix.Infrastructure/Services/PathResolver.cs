namespace PhoeNix.Infrastructure.Services;

internal static class PathResolver
{
    public static string ResolveToBase(string basePath, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return basePath;

        if (Path.IsPathRooted(path))
            return path;

        var trimmed = path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(basePath, trimmed);
    }

    public static string CombineWithBase(string basePath, string relativeOrAbsolute)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            return basePath;

        if (Path.IsPathRooted(relativeOrAbsolute))
            return relativeOrAbsolute;

        var trimmed = relativeOrAbsolute.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(basePath, trimmed);
    }
}