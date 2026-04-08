using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IModulesApiClient
{
    Task<ApiResult<PagedResponse<ModuleTemplateListResponse>>> GetModuleTemplatesAsync(
        ListModuleTemplatesRequest request,
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

    Task<ApiResult<ModuleScaffoldingResponse>> GetModuleScaffoldingAsync(
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ModuleScaffoldingResponse>> GetScaffoldingPreviewAsync(
        ModuleType type,
        List<string> testNames,
        CancellationToken cancellationToken = default);
}