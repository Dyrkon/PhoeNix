using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record GetModuleScaffoldingQuery(ModuleTemplateId ModuleTemplateId) : IQuery<ModuleScaffoldingResponse>;

internal sealed class GetModuleScaffoldingHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    INixBuildMaterializer scaffoldingProvider)
    : IQueryHandler<GetModuleScaffoldingQuery, ModuleScaffoldingResponse>
{
    public async Task<Result<ModuleScaffoldingResponse>> Handle(
        GetModuleScaffoldingQuery request,
        CancellationToken cancellationToken)
    {
        var template = await moduleTemplateRepository.GetByIdAsync(
            request.ModuleTemplateId,
            cancellationToken);

        return template
            .EnsureNotNull(ModuleErrors.NotFound(request.ModuleTemplateId))
            .Map(t =>
            {
                var moduleScaffolding = scaffoldingProvider.GetModuleScaffolding(t.Type);
                var testScaffoldings = t.Tests
                    .Select(test =>
                    {
                        var testScaffolding = scaffoldingProvider.GetTestScaffolding(test.Name);
                        return new NixTestScaffoldingDto(test.Name, testScaffolding.Prefix, testScaffolding.Suffix);
                    })
                    .ToList();

                return new ModuleScaffoldingResponse(
                    new NixModuleScaffoldingDto(moduleScaffolding.Prefix, moduleScaffolding.Suffix),
                    testScaffoldings);
            });
    }
}
