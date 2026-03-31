using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IModulesApiClient
{
    Task<ApiResult<IReadOnlyList<ModuleTemplateListResponse>>> GetModuleTemplatesAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResult<ModuleTemplateResponse>> GetModuleTemplateByIdAsync(
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> CreateModuleTemplateAsync(
        CreateModuleTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateModuleTemplateAsync(
        Guid moduleTemplateId,
        UpdateModuleTemplateRequest request,
        CancellationToken cancellationToken = default);
}