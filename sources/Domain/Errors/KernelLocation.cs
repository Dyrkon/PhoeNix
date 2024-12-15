using Domain.Shared;

namespace Domain.Errors;

public static class KernelLocation
{
    public record EmptyKernelLocation() : Error(
        $"{nameof(KernelLocation)}.EmptyKernelLocation",
        $"KernelLocation of the {nameof(KernelLocation)} cannot be empty"
    );

    public record KernelLocationTooLong() : Error(
        $"{nameof(KernelLocation)}.KernelLocationTooLong",
        $"KernelLocation of the {nameof(KernelLocation)} has to be withing {ValueObjects.KernelLocation.MaxLength}"
    );

    public record InvalidKernelLocation() : Error(
        $"{nameof(KernelLocation)}.InvalidKernelLocation",
        $"KernelLocation of the {nameof(KernelLocation)} is not a valid location"
    );
}