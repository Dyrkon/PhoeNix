using Domain.Errors;
using Domain.Shared;

namespace Domain.ValueObjects;

public record MachineName
{
    public const int MaxLength = 100;

    public required string Value { get; init; }

    private MachineName()
    {
    }

    public static Result<MachineName> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Result.Failure<MachineName>(new Machine.EmptyMachineName());
        }
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<MachineName>(new Machine.MachineNameTooLong());
        }
        
        return new MachineName
        {
            Value = value
        };
    }

    public static implicit operator string(MachineName machineName) => machineName.Value;
}