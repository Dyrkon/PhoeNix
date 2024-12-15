using System.Net.NetworkInformation;
using Carter;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Pixiecore.Queries;
using PhoeNix.Models.Pixiecore;

namespace PhoeNix.Presentation.Pixiecore;

public class PixiecoreModule() : CarterModule("/v1/boot")
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{macAddress}", GetBootInstructions)
            .Produces<PixiecoreResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> GetBootInstructions(ISender sender, CancellationToken cancellationToken,
        [FromRoute] string macAddress)
    {
        var result = PhysicalAddress.TryParse(macAddress, out var address);

        if (!result)
            Results.NotFound(new Error("404", "Not a valid MAC address."));
        var query = new GetMachineBootInstructionsQuery(address!);
        var instructionResult = await sender.Send(query, cancellationToken);

        return instructionResult.IsFailure
            ? Results.NotFound(instructionResult.Error)
            : Results.Ok(instructionResult.Value);
    }
}