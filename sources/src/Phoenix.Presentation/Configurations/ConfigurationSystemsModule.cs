using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Domain.Entities.Systems;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Configurations;

public sealed class ConfigurationSystemsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/configurations/{configurationId:guid}/systems");

        group.MapPost(string.Empty, AddSystem)
            .WithName("AddConfigurationSystem")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{systemId:guid}", UpdateSystem)
            .WithName("UpdateConfigurationSystem")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{systemId:guid}/modules", AddSystemModule)
            .WithName("AddConfigurationSystemModule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{systemId:guid}/modules/{moduleValueId:guid}", UpdateSystemModule)
            .WithName("UpdateConfigurationSystemModule")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AddSystem(
        Guid configurationId,
        CreateConfigurationSystemRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddConfigurationSystemCommand(
            new ConfigurationId(configurationId), 
            request.Name,
            request.Architecture);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateSystem(
        Guid configurationId,
        Guid systemId,
        UpdateConfigurationSystemRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConfigurationSystemCommand(
            new ConfigurationId(configurationId),
            new SystemId(systemId),
            request.Name);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> AddSystemModule(
        Guid configurationId,
        Guid systemId,
        CreateConfigurationSystemModuleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddConfigurationSystemModuleCommand(
            new ConfigurationId(configurationId),
            new SystemId(systemId),
            new ModuleTemplateId(request.ModuleTemplateId),
            request.Enabled);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateSystemModule(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        UpdateConfigurationSystemModuleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConfigurationSystemModuleCommand(
            new ConfigurationId(configurationId),
            new SystemId(systemId),
            new ModuleValueId(moduleValueId),
            request.Enabled,
            request.Entries);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}