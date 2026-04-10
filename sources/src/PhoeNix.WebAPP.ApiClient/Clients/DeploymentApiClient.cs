using PhoeNix.Application.Models.Deployment;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class DeploymentApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IDeploymentApiClient
{
    public async Task<ApiResult> UpdateMachineAsync(
        Guid configurationId,
        Guid systemId,
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var request = new UpdateMachineRequest(configurationId, systemId, machineId);
        return await PostAsync("deployment/update", request, cancellationToken);
    }
}
