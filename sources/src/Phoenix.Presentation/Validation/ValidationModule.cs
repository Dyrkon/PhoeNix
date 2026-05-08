using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Systems.Commands;
using PhoeNix.Contracts.Validation;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Validation;

public sealed class ValidationModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/validation/configurations/{configId:guid}/systems/{systemId:guid}", ScheduleSystemValidation)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet("/validation/configurations/{configId:guid}/systems/{systemId:guid}/status", GetSystemValidationStatus)
            .Produces<SystemValidationStatusResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        app.MapPost("/validation/configurations/{configId:guid}/modules/{moduleTemplateId:guid}", ScheduleModuleValidation)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet("/validation/configurations/{configId:guid}/modules/{moduleTemplateId:guid}/status", GetModuleValidationStatus)
            .Produces<ModuleValidationStatusResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }

    private static async Task<IResult> ScheduleSystemValidation(
        Guid configId,
        Guid systemId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleSystemValidationCommand(
            new ConfigurationId(configId),
            new SystemId(systemId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static IResult GetSystemValidationStatus(
        Guid configId,
        Guid systemId,
        IValidationJobTracker jobTracker)
    {
        var key = new SystemValidationKey(new ConfigurationId(configId), new SystemId(systemId));
        var status = jobTracker.GetSystemStatus(key);
        var response = new SystemValidationStatusResponse(
            status.State.ToString(),
            status.ErrorCode,
            status.ErrorMessage,
            status.Duration);
        return Results.Ok(response);
    }

    private static async Task<IResult> ScheduleModuleValidation(
        Guid configId,
        Guid moduleTemplateId,
        Architecture architecture,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleModuleValidationCommand(
            new ConfigurationId(configId),
            new ModuleTemplateId(moduleTemplateId),
            architecture);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static IResult GetModuleValidationStatus(
        Guid configId,
        Guid moduleTemplateId,
        Architecture architecture,
        IValidationJobTracker jobTracker)
    {
        var key = new ModuleValidationKey(
            new ConfigurationId(configId),
            new ModuleTemplateId(moduleTemplateId),
            architecture);
        var status = jobTracker.GetModuleStatus(key);

        var results = status.TestResults?.Select(t => new ModuleTestResultResponse(
            t.CheckAttributeName,
            t.TestName,
            t.IsSuccess,
            t.Errors.Select(e => new ModuleTestErrorResponse(e.Expected, e.Name, e.Result)).ToList()
        )).ToList();

        var response = new ModuleValidationStatusResponse(
            status.State.ToString(),
            status.ErrorCode,
            status.ErrorMessage,
            results);
        return Results.Ok(response);
    }
}
