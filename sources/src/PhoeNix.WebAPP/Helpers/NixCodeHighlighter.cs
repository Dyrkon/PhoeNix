using System.Net;
using System.Text.RegularExpressions;

namespace PhoeNix.WebAPP.Helpers;

public static class NixCodeHighlighter
{
    private const string PlaceholderColor = "#FFA500";
    private const string NameColor = "#9E9E9E";
    private const string VariableColor = "#4CAF50";
    private const string ScaffoldingColor = "#9E9E9E";

    public static string HighlightPlaceholders(string content, IEnumerable<string> placeholders)
    {
        return HighlightTerms(content, placeholders, PlaceholderColor);
    }

    public static string HighlightEntryValues(
        string content,
        IEnumerable<(string Name, string Placeholder)> entries)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var encoded = WebUtility.HtmlEncode(content);

        var entriesList = entries
            .Where(e => !string.IsNullOrEmpty(e.Placeholder) || !string.IsNullOrEmpty(e.Name))
            .OrderByDescending(e => (string.IsNullOrEmpty(e.Placeholder) ? e.Name : e.Placeholder).Length)
            .ToList();

        foreach (var (name, placeholder) in entriesList)
        {
            var searchTerm = !string.IsNullOrEmpty(placeholder) ? placeholder : name;
            var escapedTerm = Regex.Escape(WebUtility.HtmlEncode(searchTerm));
            var pattern = $@"\b{escapedTerm}\b";

            var placeholderHtml =
                $"<span style=\"color: {PlaceholderColor}; font-weight: bold;\">{WebUtility.HtmlEncode(searchTerm)}</span>";
            var nameHtml =
                $"<span style=\"color: {NameColor};\"> ({WebUtility.HtmlEncode(name)})</span>";

            var replacement = placeholderHtml + nameHtml;
            encoded = Regex.Replace(encoded, pattern, replacement);
        }

        return encoded;
    }

    public static string HighlightVariables(string content, IEnumerable<string> variables)
    {
        return HighlightTerms(content, variables, VariableColor);
    }

    public static string WrapAsScaffolding(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var encoded = WebUtility.HtmlEncode(content);
        return $"<span style=\"color: {ScaffoldingColor};\">{encoded}</span>";
    }

    public static string HtmlEncode(string content)
    {
        return WebUtility.HtmlEncode(content);
    }

    private static string HighlightTerms(string content, IEnumerable<string> terms, string color)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var encoded = WebUtility.HtmlEncode(content);

        var termsList = terms
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .OrderByDescending(t => t.Length)
            .ToList();

        foreach (var term in termsList)
        {
            var escapedTerm = Regex.Escape(WebUtility.HtmlEncode(term));
            var pattern = $@"\b{escapedTerm}\b";
            var replacement =
                $"<span style=\"color: {color}; font-weight: bold;\">{WebUtility.HtmlEncode(term)}</span>";
            encoded = Regex.Replace(encoded, pattern, replacement);
        }

        return encoded;
    }
}