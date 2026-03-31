using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class ModulesApiClient(HttpClient httpClient)
    : ApiClientBase(httpClient), IModulesApiClient
{
    public Task<ApiResult<IReadOnlyList<ModuleTemplateListResponse>>> GetModuleTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<ModuleTemplateListResponse>>("modules", cancellationToken);
    }

    public Task<ApiResult<ModuleTemplateResponse>> GetModuleTemplateByIdAsync(
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<ModuleTemplateResponse>($"modules/{moduleTemplateId}", cancellationToken);
    }

    public Task<ApiResult> CreateModuleTemplateAsync(
        CreateModuleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("modules", request, cancellationToken);
    }

    public Task<ApiResult> UpdateModuleTemplateAsync(
        Guid moduleTemplateId,
        UpdateModuleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync($"modules/{moduleTemplateId}", request, cancellationToken);
    }
}