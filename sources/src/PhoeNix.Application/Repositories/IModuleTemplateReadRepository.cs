using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IModuleTemplateReadRepository
{
    Task<PagedResponse<ModuleTemplateListResponse>> GetPageAsync(
        ListModuleTemplatesRequest request,
        UserId ownerId,
        CancellationToken cancellationToken);
}
