using System.Data.Entity;
using System.Net.NetworkInformation;
using Domain.Errors;
using Domain.Shared;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Data;
using PhoeNix.Models.Pixiecore;

namespace PhoeNix.Application.Pixiecore.Queries;

public record GetMachineBootInstructionsQuery(PhysicalAddress MacAddress) : IQuery<PixiecoreResponse>;

public class GetMachineBootInstructionsHandler(IApplicationDbContext applicationDbContext)
    : IQueryHandler<GetMachineBootInstructionsQuery, PixiecoreResponse>
{
    public async Task<Result<PixiecoreResponse>> Handle(GetMachineBootInstructionsQuery request,
        CancellationToken cancellationToken)
    {
        var bootInstructions = await applicationDbContext.Machines
            .Where(machine => machine.MacAddress == request.MacAddress)
            .Select(machine => machine.BootInstructions).FirstOrDefaultAsync(cancellationToken);

        if (bootInstructions is null)
            return Result.Failure<PixiecoreResponse>(new Machine.MachineMacNotFound(request.MacAddress));

        var bootInstruction = bootInstructions.First();
        return new PixiecoreResponse(bootInstruction.Id, bootInstruction.KernelLocation.Value.AbsoluteUri,
            bootInstruction.InitrdLocations.Select(loc => loc.Value.AbsoluteUri).ToList(),
            bootInstruction.CommandLineInstructions);
    }
}