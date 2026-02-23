namespace PhoeNix.Domain.Options;

public sealed class JwtCallbackTokenOptions
{
    public required string SigningKey { get; init; }

    public string Issuer { get; init; } = "PhoeNix";

    public string Audience { get; init; } = "PhoeNix.ProvisioningCallback";

    public TimeSpan AllowedClockSkew { get; init; } = TimeSpan.FromMinutes(2);

    public TimeSpan? MaxTtl { get; init; } = TimeSpan.FromHours(12);
}