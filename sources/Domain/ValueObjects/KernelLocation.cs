using Domain.Shared;

namespace Domain.ValueObjects;

public record KernelLocation
{
    public const int MaxLength = Constants.PxeConstants.MaxPxePath;

    public required Uri Value { get; init; }

    private KernelLocation()
    {
    }

    public static Result<KernelLocation> Create(string value)
    {
        var result = Uri.TryCreate(value, UriKind.Absolute, out var uri);
        if (!result)
            return Result.Failure<KernelLocation>(new Errors.KernelLocation.InvalidKernelLocation());

        if (string.IsNullOrEmpty(uri!.AbsoluteUri))
            return Result.Failure<KernelLocation>(new Errors.KernelLocation.EmptyKernelLocation());

        if (uri is { IsFile: true, AbsoluteUri.Length: > MaxLength })
            return Result.Failure<KernelLocation>(new Errors.KernelLocation.KernelLocationTooLong());

        return new KernelLocation
        {
            Value = uri
        };
    }

    public static implicit operator string(KernelLocation kernelLocation)
    {
        return kernelLocation.Value.AbsoluteUri;
    }
}