using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Machines.Commands;
using PhoeNix.Application.Machines.Queries;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Domain.Entities.Machines;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Machines;

public class MachinesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<CreateMachineRequest>("/machines/create", CreateMachine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/machines", GetMachines)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/machines/{machineId:guid}", GetMachineById)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> CreateMachine(
        CreateMachineRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateMachineCommand(
            request.Title,
            request.Enabled,
            request.MacAddress,
            request.Architecture,
            request.InstallDiskSelectionPreference);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private async Task<IResult> GetMachines(
        [AsParameters] ListMachinesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new ListMachinesQuery(request);
        var response = await sender.Send(query, cancellationToken);
        return response.AsHttpResult();
    }

    private async Task<IResult> GetMachineById(
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetMachineQuery(new MachineId(machineId));
        var response = await sender.Send(query, cancellationToken);
        return response.AsHttpResult();
    }
}