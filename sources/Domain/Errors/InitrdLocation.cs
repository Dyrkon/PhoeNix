using Domain.Shared;

namespace Domain.Errors;

public static class InitrdLocation
{
    public record EmptyInitrdLocation() : Error(
        $"{nameof(InitrdLocation)}.EmptyInitrdLocation",
        $"InitrdLocation of the {nameof(InitrdLocation)} cannot be empty"
    );

    public record InitrdLocationTooLong() : Error(
        $"{nameof(InitrdLocation)}.InitrdLocationTooLong",
        $"InitrdLocation of the {nameof(InitrdLocation)} has to be withing {ValueObjects.InitrdLocation.MaxLength}"
    );

    public record InvalidInitrdLocation() : Error(
        $"{nameof(InitrdLocation)}.InvalidInitrdLocation",
        $"InitrdLocation of the {nameof(InitrdLocation)} is not a valid location"
    );
}