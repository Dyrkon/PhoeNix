using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application.Options;

namespace PhoeNix.McpServer.Auth;

internal sealed class McpJwtService
{
    internal const string Issuer = "http://localhost:5003";
    internal const string Audience = "phoenix-mcp";
    private const int ExpiryHours = 8;

    private readonly JsonWebTokenHandler _handler = new();
    private readonly string _signingKey;

    public McpJwtService(IOptions<JwtCallbackTokenOptions> options)
    {
        _signingKey = options.Value.SigningKey;
    }

    public (string Token, int ExpiresIn) IssueToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddHours(ExpiryHours),
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString("D"))]),
            SigningCredentials = credentials
        };

        return (_handler.CreateToken(descriptor), ExpiryHours * 3600);
    }
}
