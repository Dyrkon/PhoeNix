using PhoeNix.Common.Models;
using PhoeNix.Contracts.Modules;
using PhoeNix.Contracts.Validation;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IModulesApiClient
{
    Task<ApiResult> ScheduleModuleValidationAsync(
        Guid configurationId,
        Guid moduleTemplateId,
        Architecture architecture,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ModuleValidationStatusResponse>> GetModuleValidationStatusAsync(
        Guid configurationId,
        Guid moduleTemplateId,
        Architecture architecture,
        CancellationToken cancellationToken = default);

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

    Task<ApiResult<ModuleTemplateResponse>> ImportModuleTemplateAsync(
        ModuleTemplateResponse request,
        CancellationToken cancellationToken = default);
}
