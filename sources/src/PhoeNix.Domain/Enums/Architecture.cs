namespace PhoeNix.Domain.Enums;

public enum Architecture
{
    X86Linux = 0,
    Aarch64Linux = 1,
    X86Darwin = 2,
    Aarch64Darwin = 3
}

public static class ArchitectureEnumExtension
{
    public static string ToArchitectureString(this Architecture architecture)
    {
        switch (architecture)
        {
            case Architecture.X86Linux:
                return "x86_64-linux";
            case Architecture.Aarch64Linux:
                return "aarch64-linux";
            case Architecture.X86Darwin:
                return "x86_64-darwin";
            case Architecture.Aarch64Darwin:
                return "aarch64-darwin";
        }

        return "x86_64-linux";
    }
}