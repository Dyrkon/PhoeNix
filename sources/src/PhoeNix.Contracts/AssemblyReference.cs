using System.Reflection;

namespace PhoeNix.Contracts;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}