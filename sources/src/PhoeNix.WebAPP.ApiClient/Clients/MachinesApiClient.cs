using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class MachinesApiClient(HttpClient httpClient)
    : ApiClientBase(httpClient), IMachinesApiClient
{
    public Task<ApiResult> CreateMachineAsync(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("machines/create", request, cancellationToken);
    }
}