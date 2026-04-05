using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Configurations.Queries;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Domain.Entities.Configurations;
using Phoenix.Presentation.Contracts;
using Phoenix.Presentation.Extensions;
using CreateConfigurationRequest = PhoeNix.Application.Models.Configurations.CreateConfigurationRequest;

namespace Phoenix.Presentation.Configurations;

public sealed class ConfigurationsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/configurations");

        group.MapGet(string.Empty, GetConfigurations)
            .WithName("GetConfigurations")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{configurationId:guid}", GetConfigurationById)
            .WithName("GetConfigurationById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost(string.Empty, CreateConfiguration)
            .WithName("CreateConfiguration")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{configurationId:guid}", UpdateConfiguration)
            .WithName("UpdateConfiguration")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{configurationId:guid}/build", BuildConfiguration)
            .WithName("BuildConfiguration")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetConfigurations(
        [AsParameters] ListConfigurationsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListConfigurationsQuery(request), cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> GetConfigurationById(
        Guid configurationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetConfigurationByIdQuery(new ConfigurationId(configurationId)),
            cancellationToken);

        return result.AsHttpResult();
    }

    private static async Task<IResult> CreateConfiguration(
        CreateConfigurationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateConfigurationCommand(request.Title, request.Description);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateConfiguration(
        Guid configurationId,
        UpdateConfigurationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConfigurationCommand(
            new ConfigurationId(configurationId),
            request.Title,
            request.Description);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> BuildConfiguration(
        Guid configurationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ExportConfigurationCommand(new ConfigurationId(configurationId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}