using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Systems.Queries;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Systems;

public class SystemsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/systems/{configurationId:guid}/system/{systemId:guid}/validate", ValidateSystem)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> ValidateSystem(Guid configurationId, Guid systemId, ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new ValidateSystemQuery(new ConfigurationId(configurationId), new SystemId(systemId));
        var result = await sender.Send(query, cancellationToken);
        return result.AsHttpResult();
    }
}