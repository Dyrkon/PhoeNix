using System.Text.RegularExpressions;

namespace PhoeNix.WebAPP.Extensions;

public static class StringExtensions
{
    public static string ToMacFormat(this string value)
    {
        var regex = string.Concat(Enumerable.Repeat("([a-fA-F0-9]{2})", 6));
        var replace = "$1:$2:$3:$4:$5:$6";
        return Regex.Replace(value, regex, replace);
    }
}