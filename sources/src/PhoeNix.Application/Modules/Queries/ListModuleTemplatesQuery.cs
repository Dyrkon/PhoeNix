using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record ListModuleTemplatesQuery(ListModuleTemplatesRequest Request)
    : IQuery<PagedResponse<ModuleTemplateListResponse>>;

internal sealed class ListModuleTemplatesQueryHandler(
    IModuleTemplateReadRepository moduleTemplateReadRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<ListModuleTemplatesQuery, PagedResponse<ModuleTemplateListResponse>>
{
    public Task<Result<PagedResponse<ModuleTemplateListResponse>>> Handle(
        ListModuleTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Task.FromResult(Result.Failure<PagedResponse<ModuleTemplateListResponse>>(userIdResult.Error));

        var userId = userIdResult.Value;

        return Result.Success(request.Request)
            .Ensure(r => r.Page > 0, new Error("Modules.InvalidPage", "Page must be greater than zero."))
            .Ensure(r => r.PageSize > 0, new Error("Modules.InvalidPageSize", "Page size must be greater than zero."))
            .Map(r => moduleTemplateReadRepository.GetPageAsync(r, userId, cancellationToken));
    }
}
