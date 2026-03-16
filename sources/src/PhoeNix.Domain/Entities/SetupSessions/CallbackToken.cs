using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Domain.Entities.SetupSessions;

public record CallbackToken(
    string Token,
    DateTime ExpiresAtUtc,
    DateTime? RevokedAtUtc
);

public readonly record struct CallbackTokenContext(
    SetupSessionId SessionId,
    MachineId MachineId,
    DateTime ExpiresAtUtc
);

public static class CallbackTokenExtensions
{
    public static bool IsValid(this CallbackToken token, DateTime nowUtc)
    {
        return token.RevokedAtUtc is null && token.ExpiresAtUtc > nowUtc;
    }
}