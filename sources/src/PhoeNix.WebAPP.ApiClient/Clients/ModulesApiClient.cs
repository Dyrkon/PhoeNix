using PhoeNix.Common.Models;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
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
        return PostAsync("modules/templates/new", request, cancellationToken);
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

    public Task<ApiResult<ModuleScaffoldingResponse>> GetScaffoldingPreviewAsync(
        ModuleType type,
        List<string> testNames,
        CancellationToken cancellationToken = default)
    {
        var testNamesParam = testNames.Count > 0 ? string.Join(",", testNames) : string.Empty;
        var url = $"modules/scaffolding/preview?type={type}&testNames={Uri.EscapeDataString(testNamesParam)}";
        return GetAsync<ModuleScaffoldingResponse>(url, cancellationToken);
    }

    public Task<ApiResult<ModuleTemplateResponse>> ImportModuleTemplateAsync(
        ModuleTemplateResponse request,
        CancellationToken cancellationToken = default)
    {
        return PostWithResponseAsync<ModuleTemplateResponse>("modules/import", request, cancellationToken);
    }
}
