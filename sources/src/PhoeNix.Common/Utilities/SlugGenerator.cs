using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PhoeNix.Common.Utilities;

public static partial class SlugGenerator
{
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            sb.Append(c);
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        result = result.ToLowerInvariant();
        result = SpacesAndUnderscoresRegex().Replace(result, "-");
        result = NonAlphanumericRegex().Replace(result, "");
        result = ConsecutiveHyphensRegex().Replace(result, "-");
        result = result.Trim('-');

        return result;
    }

    [GeneratedRegex(@"[\s_]+")]
    private static partial Regex SpacesAndUnderscoresRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex ConsecutiveHyphensRegex();
}
