using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class MachinesApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IMachinesApiClient
{
    public async Task<ApiResult> CreateMachineAsync(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync("machines/create", request, cancellationToken);
    }

    public async Task<ApiResult<IEnumerable<MachineListResponse>>> GetMachinesAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<IEnumerable<MachineListResponse>>("machines", cancellationToken);
    }
}