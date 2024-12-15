using Domain.Entities.BootInstruction;

namespace PhoeNix.Models.Machines;

public record BootInstructionResponse(
    BootInstructionId Id,
    string KernelLocation,
    List<string> InitrdLocations,
    string CommandLineInstructions);