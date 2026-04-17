using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using Phoenix.Presentation.Contracts;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Configurations;

public sealed class ConfigurationInputsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/configurations/{configurationId:guid}/inputs");

        group.MapPost(string.Empty, AddInput)
            .WithName("AddConfigurationInput")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{inputId:guid}", UpdateInput)
            .WithName("UpdateConfigurationInput")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{inputId:guid}", RemoveInput)
            .WithName("RemoveConfigurationInput")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AddInput(
        Guid configurationId,
        CreateConfigurationInputRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddConfigurationInputCommand(new ConfigurationId(configurationId), request.Source,
            request.Name, request.Follows);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateInput(
        Guid configurationId,
        Guid inputId,
        UpdateConfigurationInputRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateConfigurationInputCommand(new ConfigurationId(configurationId), new InputId(inputId),
            request.Source, request.Name, request.Follows);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> RemoveInput(
        Guid configurationId,
        Guid inputId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RemoveConfigurationInputCommand(new ConfigurationId(configurationId), new InputId(inputId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}