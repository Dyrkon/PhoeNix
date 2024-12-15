using Domain.Shared;

namespace Domain.ValueObjects;

public record InitrdLocation
{
    public const int MaxLength = Constants.PxeConstants.MaxPxePath;

    public required Uri Value { get; init; }

    private InitrdLocation()
    {
    }

    public static Result<InitrdLocation> Create(string value)
    {
        var result = Uri.TryCreate(value, UriKind.Absolute, out var uri);
        if (!result)
            return Result.Failure<InitrdLocation>(new Errors.InitrdLocation.InvalidInitrdLocation());

        if (string.IsNullOrEmpty(uri!.AbsoluteUri))
            return Result.Failure<InitrdLocation>(new Errors.InitrdLocation.EmptyInitrdLocation());

        if (uri is { IsFile: true, AbsoluteUri.Length: > MaxLength })
            return Result.Failure<InitrdLocation>(new Errors.InitrdLocation.InitrdLocationTooLong());

        return new InitrdLocation
        {
            Value = uri
        };
    }

    public static implicit operator string(InitrdLocation initrdLocation)
    {
        return initrdLocation.Value.AbsoluteUri;
    }
}