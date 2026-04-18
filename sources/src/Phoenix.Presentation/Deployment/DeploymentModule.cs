using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Models.Deployment;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Deployment;

public sealed class DeploymentModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<UpdateMachineRequest>("/deployment/update", UpdateMachine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/deployment/status/{machineId:guid}", GetDeploymentStatus)
            .Produces<DeploymentStatusResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> UpdateMachine(UpdateMachineRequest request, ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMachineConfiguration(
            new ConfigurationId(request.ConfigurationId),
            new MachineId(request.MachineId),
            new SystemId(request.SystemId));

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static IResult GetDeploymentStatus(Guid machineId, IDeploymentJobTracker jobTracker)
    {
        var status = jobTracker.GetStatus(new MachineId(machineId));
        var response = new DeploymentStatusResponse(
            status.State.ToString(),
            status.ErrorCode,
            status.ErrorMessage);
        return Results.Ok(response);
    }
}