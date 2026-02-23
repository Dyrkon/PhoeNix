using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Provosioning.Commands;
using PhoeNix.Domain.Entities.Machines;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Provisioning;

public class ProvisioningModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/provisioning/machine/{machineId:guid}/start", StartProvisioning)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> StartProvisioning(Guid machineId, ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new StartProvisioningSessionCommand(new MachineId(machineId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}