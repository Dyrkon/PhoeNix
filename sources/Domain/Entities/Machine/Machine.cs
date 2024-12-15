using System.Net.NetworkInformation;
using Domain.Enums;
using Domain.Primitives;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Entities.Machine;

public class Machine : AggregateRoot<MachineId>
{
    private Machine(MachineId id, MachineName machineName) : base(id)
    {
        MachineName = machineName;
    }

    public PhysicalAddress MacAddress { get; private set; }
    public MachineName MachineName { get; private set; }
    public MachineState MachineState { get; private set; }

    public List<BootInstruction.BootInstruction> BootInstructions { get; private set; }

    public static Result<Machine> Create(MachineId id, MachineName name, PhysicalAddress macAddress,
        List<BootInstruction.BootInstruction> bootInstructions, MachineState machineState)
    {
        return new Machine(id, name)
        {
            MachineState = machineState,
            MacAddress = macAddress,
            BootInstructions = bootInstructions
        };
    }
}