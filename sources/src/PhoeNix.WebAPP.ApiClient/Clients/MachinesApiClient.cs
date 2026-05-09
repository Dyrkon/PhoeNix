using PhoeNix.Common.Models;
using PhoeNix.Contracts.Machines;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Helpers;
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

    public async Task<ApiResult<PagedResponse<MachineListResponse>>> GetMachinesAsync(
        ListMachinesRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryString = QueryStringBuilder.BuildFrom(request);
        return await GetAsync<PagedResponse<MachineListResponse>>($"machines{queryString}", cancellationToken);
    }

    public async Task<ApiResult<MachineDetailResponse>> GetMachineAsync(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<MachineDetailResponse>($"machines/{machineId}", cancellationToken);
    }

    public async Task<ApiResult> UpdateMachineAsync(
        Guid machineId,
        UpdateMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync($"machines/{machineId}", request, cancellationToken);
    }
}
