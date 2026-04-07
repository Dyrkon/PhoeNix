using System.Text;

namespace PhoeNix.WebAPP.Helpers;

public static class NixCodeFormatter
{
    private const int IndentSize = 2;

    public static string Format(string content)
    {
        return Format(content, 0).FormattedContent;
    }

    public static (string FormattedContent, int EndingIndentLevel) Format(string content, int startingIndentLevel)
    {
        if (string.IsNullOrEmpty(content))
            return (string.Empty, startingIndentLevel);

        var withoutNewlines = content
            .Replace("\r\n", " ")
            .Replace("\n", "")
            .Replace("\r", " ");

        while (withoutNewlines.Contains("  "))
            withoutNewlines = withoutNewlines.Replace("  ", " ");

        var result = new StringBuilder();
        var indentLevel = startingIndentLevel;
        var currentLine = new StringBuilder();

        foreach (var c in withoutNewlines)
            switch (c)
            {
                case '}':
                    FlushLine(result, currentLine, indentLevel);
                    indentLevel = Math.Max(0, indentLevel - 1);
                    currentLine.Append(c);
                    break;
                case '{':
                    currentLine.Append(c);
                    FlushLine(result, currentLine, indentLevel);
                    indentLevel++;
                    break;
                case ':' or ';':
                    currentLine.Append(c);
                    FlushLine(result, currentLine, indentLevel);
                    break;
                default:
                    currentLine.Append(c);
                    break;
            }

        FlushLine(result, currentLine, indentLevel);

        return (result.ToString().TrimEnd('\n'), indentLevel);
    }

    private static void FlushLine(StringBuilder result, StringBuilder currentLine, int indentLevel)
    {
        var line = currentLine.ToString().Trim();
        if (!string.IsNullOrEmpty(line))
        {
            var indent = new string(' ', indentLevel * IndentSize);
            result.Append(indent);
            result.Append(line);
            result.Append('\n');
        }

        currentLine.Clear();
    }
}