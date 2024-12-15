using Domain.Entities.BootInstruction;

namespace PhoeNix.Models.Pixiecore;

public record PixiecoreResponse(BootInstructionId Id, string Kernel, List<string> InitrdList, string? CmdLine = null);