using Domain.Shared;

namespace Domain.ValueObjects;

public record CommandLineInstructions
{
    public const int MaxLength = Constants.PxeConstants.MaxPxeCmdLenght;

    public required string Value { get; init; }

    private CommandLineInstructions()
    {
    }

    public static Result<CommandLineInstructions> Create(string value)
    {
        if (value.Length > MaxLength)
            return Result.Failure<CommandLineInstructions>(
                new Errors.CommandLineInstructions.CommandLineInstructionTooLong());

        return new CommandLineInstructions
        {
            Value = value
        };
    }

    public static CommandLineInstructions CreateEmpty()
    {
        return new CommandLineInstructions
        {
            Value = ""
        };
    }

    public static implicit operator string(CommandLineInstructions commandLineInstructions)
    {
        return commandLineInstructions.Value;
    }
}