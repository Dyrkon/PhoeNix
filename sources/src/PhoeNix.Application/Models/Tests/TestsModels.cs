using System.Text.Json;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Application.Models.Tests;

public record ModuleTestResponse(TestId Id, string Name, bool IsSuccess, IReadOnlyList<ModuleTestErrorResponse> Errors);

public record ModuleTestErrorResponse(string Expected, string Name, string Result);

public record SystemTestResponse(SystemId Id, bool IsSuccess, string BuildTime);

public static class ModuleTestParser
{
    public static IReadOnlyList<ModuleTestErrorResponse> ParseFailures(string json)
    {
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a JSON array.");

        var failures = new List<ModuleTestErrorResponse>();

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var name = item.TryGetProperty("name", out var n)
                ? n.ValueKind == JsonValueKind.String ? n.GetString() ?? "" : n.ToString()
                : "<unnamed>";

            var expected = item.TryGetProperty("expected", out var e) ? ToUiString(e) : "<missing>";
            var result = item.TryGetProperty("result", out var r) ? ToUiString(r) : "<missing>";

            if (!string.Equals(expected, result, StringComparison.Ordinal))
                failures.Add(new ModuleTestErrorResponse(
                    expected,
                    name,
                    result));
        }

        return failures;
    }

    private static string ToUiString(JsonElement el)
    {
        return el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? ""
            : el.ToString();
    }
}