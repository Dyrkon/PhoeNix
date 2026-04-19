using PhoeNix.Contracts.Machines;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class MetricsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IMetricsApiClient
{
    public async Task<ApiResult<MachineMetricsResponse>> GetMachineMetricsAsync(
        Guid machineId,
        string range = "24h",
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<MachineMetricsResponse>($"machines/{machineId}/metrics?range={range}", cancellationToken);
    }
}
