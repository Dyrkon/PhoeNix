using Domain.Entities.BootInstruction;
using PhoeNix.Models.Machines;

namespace PhoeNix.Application.Mappings;

public static class BootInstructionMappings
{
    public static BootInstructionResponse MapBootInstructionToDto(BootInstruction bootInstruction)
    {
        return new BootInstructionResponse(bootInstruction.Id, bootInstruction.KernelLocation,
            bootInstruction.InitrdLocations.Select(i => i.Value.AbsoluteUri).ToList(),
            bootInstruction.CommandLineInstructions);
    }
}