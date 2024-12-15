using Domain.Shared;

namespace Domain.Errors;

public static class CommandLineInstructions
{
    public record CommandLineInstructionTooLong() : Error(
        $"{nameof(CommandLineInstructions)}.CommandLineInstructionTooLong",
        $"CommandLineInstructions of the {nameof(CommandLineInstructions)} has to be withing {ValueObjects.CommandLineInstructions.MaxLength}"
    );
}