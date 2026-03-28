using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record GetModuleTemplateByIdQuery(ModuleTemplateId ModuleTemplateId) : IQuery<ModuleTemplateResponse>;

internal sealed class GetModuleTemplateByIdHandler(
    IModuleTemplateRepository moduleTemplateRepository)
    : IQueryHandler<GetModuleTemplateByIdQuery, ModuleTemplateResponse>
{
    public async Task<Result<ModuleTemplateResponse>> Handle(
        GetModuleTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var template = await moduleTemplateRepository.GetByIdAsync(
            request.ModuleTemplateId,
            cancellationToken);

        return template
            .EnsureNotNull(ModuleErrors.NotFound(request.ModuleTemplateId))
            .Map(ModuleMappings.MapModuleToDto);
    }
}