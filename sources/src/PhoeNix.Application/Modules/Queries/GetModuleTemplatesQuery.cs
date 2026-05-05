using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record GetModuleTemplatesQuery() : IQuery<IReadOnlyList<ModuleTemplateListResponse>>;

internal sealed class GetModuleTemplatesHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetModuleTemplatesQuery, IReadOnlyList<ModuleTemplateListResponse>>
{
    public async Task<Result<IReadOnlyList<ModuleTemplateListResponse>>> Handle(
        GetModuleTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<IReadOnlyList<ModuleTemplateListResponse>>(userIdResult.Error);

        var templates = await moduleTemplateRepository.GetAllAsync(userIdResult.Value, cancellationToken);

        return Result.Success<IReadOnlyList<ModuleTemplateListResponse>>(
            templates.Select(ModuleMappings.MapModuleToListDto).ToList());
    }
}