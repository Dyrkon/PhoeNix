using System.Globalization;
using System.Reflection;

namespace PhoeNix.WebAPP.ApiClient.Helpers;

public static class QueryStringBuilder
{
    public static string BuildFrom(object request)
    {
        var queryParts = request
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => BuildQueryPart(property, request))
            .Where(part => part is not null)
            .Cast<string>()
            .ToList();

        return queryParts.Count == 0
            ? string.Empty
            : $"?{string.Join("&", queryParts)}";
    }

    private static string? BuildQueryPart(PropertyInfo property, object request)
    {
        var value = property.GetValue(request);
        if (value is null)
            return null;

        if (value is string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            return $"{ToCamelCase(property.Name)}={Uri.EscapeDataString(stringValue)}";
        }

        if (value is bool boolValue)
            return $"{ToCamelCase(property.Name)}={boolValue.ToString().ToLowerInvariant()}";

        if (value is Enum)
            return $"{ToCamelCase(property.Name)}={value}";

        return value switch
        {
            int intValue => $"{ToCamelCase(property.Name)}={intValue.ToString(CultureInfo.InvariantCulture)}",
            long longValue => $"{ToCamelCase(property.Name)}={longValue.ToString(CultureInfo.InvariantCulture)}",
            Guid guidValue => $"{ToCamelCase(property.Name)}={guidValue}",
            _ =>
                $"{ToCamelCase(property.Name)}={Uri.EscapeDataString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)}"
        };
    }

    private static string ToCamelCase(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }
}