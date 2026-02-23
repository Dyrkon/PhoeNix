using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Modules.Queries;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Modules;

public class ModulesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/modules{configurationId:guid}/module/{moduleId:guid}/architecture/{architecture:int}/validate",
                ValidateModule)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        app.MapPost("/modules/create", CreateModuleTemplate)
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

    private async Task<IResult> CreateModuleTemplate(string name, bool enabled, ModuleType moduleType,
        List<Architecture> architectures, ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new AddModuleTemplateCommand(name, enabled, moduleType, architectures);
        var result = await sender.Send(query, cancellationToken);
        return result.AsHttpResult();
    }
}