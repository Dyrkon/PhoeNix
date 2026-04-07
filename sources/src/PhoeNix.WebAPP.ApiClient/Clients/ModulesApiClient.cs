using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Helpers;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class ModulesApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IModulesApiClient
{
    public Task<ApiResult<PagedResponse<ModuleTemplateListResponse>>> GetModuleTemplatesAsync(
        ListModuleTemplatesRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryString = QueryStringBuilder.BuildFrom(request);
        return GetAsync<PagedResponse<ModuleTemplateListResponse>>($"modules{queryString}", cancellationToken);
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

    public Task<ApiResult<ModuleScaffoldingResponse>> GetModuleScaffoldingAsync(
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<ModuleScaffoldingResponse>($"modules/{moduleTemplateId}/scaffolding", cancellationToken);
    }
}