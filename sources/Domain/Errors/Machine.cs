using System.Net.NetworkInformation;
using Domain.Entities.Machine;
using Domain.Shared;
using Domain.ValueObjects;

namespace Domain.Errors;

public static class Machine
{
    public record EmptyMachineName() : Error(
        $"{nameof(Machine)}.EmptyMachineName",
        $"MachineName of the {nameof(Machine)} cannot be empty"
    );

    public record MachineNameTooLong() : Error(
        $"{nameof(Machine)}.MachineNameTooLong",
        $"MachineName of the {nameof(Machine)} has to be withing {MachineName.MaxLength}"
    );

    public record MachineMacNotFound(PhysicalAddress MacAddress) : NotFoundError(
        $"{nameof(Machine)}.MachineNotFound",
        $"Machine with MAC address {MacAddress} was not found"
    );

    public record MachineNotFound(MachineId MachineId) : NotFoundError(
        $"{nameof(Machine)}.MachineNotFound",
        $"Machine with id {MachineId} was not found"
    );
}