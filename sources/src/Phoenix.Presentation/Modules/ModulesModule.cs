using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Modules.Queries;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Modules;

public class ModulesModule : CarterModule
{
    public ModulesModule() : base("/modules")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{configurationId:guid}/module/{moduleId:guid}/architecture/{architecture:int}/validate",
                ValidateModule)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> ValidateModule(Guid configurationId, Guid moduleId, int architecture, ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new ValidateModuleQuery(new ConfigurationId(configurationId), new ModuleTemplateId(moduleId),
            (Architecture)architecture);
        var result = await sender.Send(query, cancellationToken);
        return result.AsHttpResult();
    }
}