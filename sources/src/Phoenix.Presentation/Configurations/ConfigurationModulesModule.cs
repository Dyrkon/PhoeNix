using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using Phoenix.Presentation.Contracts;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Configurations;

public sealed class ConfigurationModulesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/configurations/{configurationId:guid}/modules");

        group.MapPost(string.Empty, AddModule)
            .WithName("AddConfigurationModule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{moduleValueId:guid}", UpdateModule)
            .WithName("UpdateConfigurationModule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AddModule(
        Guid configurationId,
        CreateConfigurationModuleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddConfigurationModuleCommand(new ConfigurationId(configurationId),
            new ModuleTemplateId(request.ModuleTemplateId), request.Enabled);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateModule(
        Guid configurationId,
        Guid moduleValueId,
        UpdateConfigurationModuleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConfigurationModuleCommand(new ConfigurationId(configurationId),
            new ModuleValueId(moduleValueId), request.Enabled, request.Entries);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}