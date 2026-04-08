using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public sealed record GetScaffoldingPreviewQuery(ModuleType Type, List<string> TestNames) : IQuery<ModuleScaffoldingResponse>;

internal sealed class GetScaffoldingPreviewHandler(INixBuildMaterializer scaffoldingProvider)
    : IQueryHandler<GetScaffoldingPreviewQuery, ModuleScaffoldingResponse>
{
    public Task<Result<ModuleScaffoldingResponse>> Handle(
        GetScaffoldingPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var moduleScaffolding = scaffoldingProvider.GetModuleScaffolding(request.Type);
        var testScaffoldings = request.TestNames
            .Select(testName =>
            {
                var testScaffolding = scaffoldingProvider.GetTestScaffolding(testName);
                return new NixTestScaffoldingDto(testName, testScaffolding.Prefix, testScaffolding.Suffix);
            })
            .ToList();

        var response = new ModuleScaffoldingResponse(
            new NixModuleScaffoldingDto(moduleScaffolding.Prefix, moduleScaffolding.Suffix),
            testScaffoldings);

        return Task.FromResult(Result.Success(response));
    }
}
