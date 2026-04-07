using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record ListModuleTemplatesQuery(ListModuleTemplatesRequest Request)
    : IQuery<PagedResponse<ModuleTemplateListResponse>>;

internal sealed class ListModuleTemplatesQueryHandler(
    IModuleTemplateReadRepository moduleTemplateReadRepository)
    : IQueryHandler<ListModuleTemplatesQuery, PagedResponse<ModuleTemplateListResponse>>
{
    public async Task<Result<PagedResponse<ModuleTemplateListResponse>>> Handle(
        ListModuleTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Page <= 0)
            return Result.Failure<PagedResponse<ModuleTemplateListResponse>>(new Error(
                "Modules.InvalidPage",
                "Page must be greater than zero."));

        if (request.Request.PageSize <= 0)
            return Result.Failure<PagedResponse<ModuleTemplateListResponse>>(new Error(
                "Modules.InvalidPageSize",
                "Page size must be greater than zero."));

        var response = await moduleTemplateReadRepository.GetPageAsync(request.Request, cancellationToken);

        return Result.Success(response);
    }
}
