using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Machines.Commands;
using PhoeNix.Application.Models.Machines;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Machines;

public class MachinesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<CreateMachineRequest>("/machines/create", CreateMachine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> CreateMachine(CreateMachineRequest request, ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateMachineCommand(request.Title, request.Enabled, request.MacAddress);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}