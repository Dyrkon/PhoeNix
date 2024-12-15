using Domain.Entities.Machine;
using PhoeNix.Models.Machines;
using PhoeNix.Models.Pixiecore;

namespace PhoeNix.Application.Mappings;

public static class MachineMappings
{
    public static MachineResponse MapMachineToDto(Machine machine)
    {
        return new MachineResponse(
            machine.Id,
            machine.MacAddress.ToString(),
            machine.MachineName,
            (int)machine.MachineState,
            machine.BootInstructions.Select(BootInstructionMappings.MapBootInstructionToDto).ToList());
    }
}