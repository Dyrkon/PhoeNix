using System.Collections.Concurrent;

namespace PhoeNix.McpServer.Auth;

internal sealed class McpAuthCodeStore
{
    private readonly ConcurrentDictionary<string, McpAuthCode> _codes = new();

    public void Store(string code, Guid userId, string codeChallenge)
        => _codes[code] = new McpAuthCode(userId, codeChallenge, DateTime.UtcNow.AddMinutes(5));

    public McpAuthCode? Consume(string code)
    {
        if (_codes.TryRemove(code, out var entry) && entry.Expiry > DateTime.UtcNow)
            return entry;
        return null;
    }
}

internal sealed record McpAuthCode(Guid UserId, string CodeChallenge, DateTime Expiry);
