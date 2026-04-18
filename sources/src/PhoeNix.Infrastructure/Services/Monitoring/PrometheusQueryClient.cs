using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Repositories;

namespace PhoeNix.Infrastructure.Services.Monitoring;

internal sealed class PrometheusQueryClient : IPrometheusQueryClient
{
    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PrometheusQueryClient(HttpClient httpClient, IServiceScopeFactory scopeFactory)
    {
        _httpClient = httpClient;
        _scopeFactory = scopeFactory;
    }

    public async Task<double?> QueryInstantAsync(string promQl, CancellationToken cancellationToken = default)
    {
        var baseAddress = await GetBaseAddressAsync(cancellationToken);
        var encoded = Uri.EscapeDataString(promQl);
        var url = $"{baseAddress}api/v1/query?query={encoded}";

        using var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
            return null;

        var response = await httpResponse.Content.ReadFromJsonAsync<PrometheusResponse>(JsonOptions, cancellationToken);

        if (response?.Status != "success" || response.Data?.Result is not { Count: > 0 } results)
            return null;

        var valueArr = results[0].Value;
        if (valueArr is null || valueArr.Count < 2)
            return null;

        return TryParseDouble(valueArr[1]);
    }

    public async Task<PrometheusRangeSeries> QueryRangeAsync(
        string promQl,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan step,
        CancellationToken cancellationToken = default)
    {
        var baseAddress = await GetBaseAddressAsync(cancellationToken);
        var encoded = Uri.EscapeDataString(promQl);
        var startUnix = start.ToUnixTimeSeconds();
        var endUnix = end.ToUnixTimeSeconds();
        var stepSeconds = Math.Max(1, (long)step.TotalSeconds);

        var url = $"{baseAddress}api/v1/query_range?query={encoded}&start={startUnix}&end={endUnix}&step={stepSeconds}";

        using var httpResponse = await _httpClient.GetAsync(url, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
            return new PrometheusRangeSeries([], []);

        var response = await httpResponse.Content.ReadFromJsonAsync<PrometheusResponse>(JsonOptions, cancellationToken);

        if (response?.Status != "success" || response.Data?.Result is not { Count: > 0 } results)
            return new PrometheusRangeSeries([], []);

        var values = results[0].Values;
        if (values is null || values.Count == 0)
            return new PrometheusRangeSeries([], []);

        var timestamps = new DateTimeOffset[values.Count];
        var points = new double?[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var pair = values[i];
            if (pair.Count >= 2)
            {
                var ts = TryParseDouble(pair[0]);
                timestamps[i] = ts.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds((long)ts.Value)
                    : DateTimeOffset.MinValue;
                points[i] = TryParseDouble(pair[1]);
            }
        }

        return new PrometheusRangeSeries(timestamps, points);
    }

    private async Task<string> GetBaseAddressAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = await repo.GetAsync(cancellationToken);
        var endpoint = settings?.MonitoringPrometheusEndpoint ?? "http://localhost:9090/prometheus";
        return endpoint.TrimEnd('/') + "/";
    }

    private static double? TryParseDouble(JsonElement? element)
    {
        if (element is null)
            return null;

        return element.Value.ValueKind switch
        {
            JsonValueKind.Number => element.Value.GetDouble(),
            JsonValueKind.String when double.TryParse(element.Value.GetString(), out var d) => d,
            _ => null
        };
    }

    private sealed record PrometheusResponse(
        string Status,
        PrometheusData? Data);

    private sealed record PrometheusData(
        string ResultType,
        List<PrometheusResult> Result);

    private sealed record PrometheusResult(
        [property: JsonPropertyName("value")] List<JsonElement>? Value,
        [property: JsonPropertyName("values")] List<List<JsonElement>>? Values);
}