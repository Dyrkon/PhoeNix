using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record GetModuleTemplatesQuery() : IQuery<IReadOnlyList<ModuleTemplateListResponse>>;

internal sealed class GetModuleTemplatesHandler(
    IModuleTemplateRepository moduleTemplateRepository)
    : IQueryHandler<GetModuleTemplatesQuery, IReadOnlyList<ModuleTemplateListResponse>>
{
    public async Task<Result<IReadOnlyList<ModuleTemplateListResponse>>> Handle(
        GetModuleTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await moduleTemplateRepository.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ModuleTemplateListResponse>>(
            templates.Select(ModuleMappings.MapModuleToListDto).ToList());
    }
}