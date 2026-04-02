using PhoeNix.Application.Models.Machines;
using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Application.Mappings;

public static class MachineMapping
{
    public static MachineListResponse MapMachineToDto(Machine machine)
    {
        return new MachineListResponse(
            machine.Id.Value,
            machine.Title,
            machine.Enabled,
            machine.MacAddress.ToString(),
            machine.Architecture,
            machine.MachineStatus.MachineState);
    }
}