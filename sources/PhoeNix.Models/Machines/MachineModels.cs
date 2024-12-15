using System.Net.NetworkInformation;
using Domain.Entities.Machine;
using Domain.Enums;

namespace PhoeNix.Models.Machines;

public record MachineListResponse();

public record MachineResponse(
    MachineId MachineId,
    string MacAddress,
    string MachineName,
    int MachineState,
    List<BootInstructionResponse> BootInstructions);