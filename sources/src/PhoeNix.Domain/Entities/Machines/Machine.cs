using System.Net.NetworkInformation;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public class Machine : AggregateRoot<MachineId>
{
    private Machine(MachineId id) : base(id)
    {
    }

    public string Title { get; private set; }

    public bool Enabled { get; private set; }

    public PhysicalAddress MacAddress { get; private set; }

    public HardwareProfile? HardwareProfile { get; private set; }

    public SoftwareSnapshot? SoftwareSnapshot { get; private set; }

    public MachineStatus MachineStatus { get; private set; }

    public Result ChangeMacAddress(string addressString)
    {
        if (!PhysicalAddress.TryParse(addressString, out var address))
            return Result.Failure(new Error("MachineMACError", $"Unable to parse machine MAC address {addressString}"));

        MacAddress = address;
        return Result.Success();
    }

    public Result Enable()
    {
        if (Enabled)
            return Result.Failure(new Error("MachineAlreadyEnabled", $"Machine {Title} is enabled already"));
        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (!Enabled)
            return Result.Failure(new Error("MachineAlreadyDisabled", $"Machine {Title} is disabled already"));
        Enabled = false;
        return Result.Success();
    }

    public Result ChangeMachineState(MachineState machineState, DateTime now)
    {
        return MachineStatus.ChangeMachineState(machineState, now);
    }

    public static Result<Machine> Create(MachineId machineId, string macAddress, string title, bool enabled)
    {
        return new Result<Machine>(true, Error.None,
                new Machine(machineId)
                {
                    Title = title, Enabled = enabled,
                    MachineStatus = new MachineStatus(MachineState.Registered)
                })
            .Tap(machine => machine.ChangeMacAddress(macAddress));
    }
}