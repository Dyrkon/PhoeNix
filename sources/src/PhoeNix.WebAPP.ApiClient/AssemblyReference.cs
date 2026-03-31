using System.Reflection;

namespace PhoeNix.WebAPP.ApiClient;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}