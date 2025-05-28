using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Domain.Models.Configurations;

namespace Phoenix.Presentation.Configurations;

public class ConfigurationsModule : CarterModule
{
    public ConfigurationsModule() : base("/configurations")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{configurationId:guid}/build", BuildConfiguration)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/create", CreateConfiguration)
            .Produces(StatusCodes.Status200OK);
        
        app.MapDelete("/delete", DeleteConfiguration)
            .Produces(StatusCodes.Status200OK);
    }

    private async Task<IResult> BuildConfiguration(Guid configurationId, ISender sender, CancellationToken cancellationToken)
    {
        return Results.Problem();
    }

    private async Task<IResult> CreateConfiguration(CreateConfigurationRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return Results.Problem();
    }
    
    private async Task<IResult> DeleteConfiguration(Guid configurationId, ISender sender, CancellationToken cancellationToken)
    {
        return Results.Problem();
    }
}