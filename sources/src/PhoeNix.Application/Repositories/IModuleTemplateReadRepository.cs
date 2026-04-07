using PhoeNix.Application.Models.Modules;
using PhoeNix.Common.Models;

namespace PhoeNix.Application.Repositories;

public interface IModuleTemplateReadRepository
{
    Task<PagedResponse<ModuleTemplateListResponse>> GetPageAsync(
        ListModuleTemplatesRequest request,
        CancellationToken cancellationToken);
}
