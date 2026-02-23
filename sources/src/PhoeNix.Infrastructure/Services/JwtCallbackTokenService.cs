using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Options;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class JwtCallbackTokenService : ICallbackTokenService
{
    private readonly JsonWebTokenHandler _handler = new();
    private readonly JwtCallbackTokenOptions _options;
    private readonly TokenValidationParameters _validationParameters;

    public JwtCallbackTokenService(IOptions<JwtCallbackTokenOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SigningKey))
            throw new InvalidOperationException("JwtCallbackTokenOptions.SigningKey must be configured.");

        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("JwtCallbackTokenOptions.Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("JwtCallbackTokenOptions.Audience must be configured.");

        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,

            ValidateAudience = true,
            ValidAudience = _options.Audience,

            ValidateLifetime = true,

            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            ClockSkew = _options.AllowedClockSkew
        };
    }

    public Result<CallbackToken> Create(
        ProvisioningSessionId sessionId,
        MachineId machineId,
        DateTime nowUtc,
        TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero)
            return Result.Failure<CallbackToken>(new Error("JwtTtl", "TTL must be positive."));

        if (_options.MaxTtl is not null && ttl > _options.MaxTtl.Value)
            return Result.Failure<CallbackToken>(new Error("JwtTtl", $"TTL exceeds MaxTtl ({_options.MaxTtl})."));

        var expiresAtUtc = nowUtc.Add(ttl);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, machineId.Value.ToString("D")),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("D")),
            new Claim("sid", sessionId.Value.ToString("D")),
            new Claim("mid", machineId.Value.ToString("D"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,

            NotBefore = nowUtc,
            IssuedAt = nowUtc,
            Expires = expiresAtUtc,

            Subject = new ClaimsIdentity(claims),

            SigningCredentials = credentials
        };

        var tokenString = _handler.CreateToken(descriptor);
        return new CallbackToken(tokenString, expiresAtUtc, null);
    }

    public async Task<Result<CallbackTokenContext>> ValidateAndDecode(string token, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenMissing",
                "Callback token is missing."));

        var result = await _handler.ValidateTokenAsync(token, _validationParameters);

        if (!result.IsValid)
            return Result.Failure<CallbackTokenContext>(new Error(
                "CallbackTokenInvalid",
                "Callback token is invalid."
            ));

        var identity = result.ClaimsIdentity;
        if (identity is null)
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenInvalid",
                "Callback token is invalid."));

        var sid = identity.FindFirst("sid")?.Value;
        var mid = identity.FindFirst("mid")?.Value;

        if (!Guid.TryParse(sid, out var sessionGuid))
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenInvalidSessionId",
                "Invalid session id in token."));

        if (!Guid.TryParse(mid, out var machineGuid))
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenInvalidMachineId",
                "Invalid machine id in token."));

        var expValue = identity.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime expiresAtUtc;

        if (long.TryParse(expValue, out var expUnix))
            expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        else
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenInvalidExpiry",
                "Invalid expiry in token."));

        if (expiresAtUtc <= nowUtc - _options.AllowedClockSkew)
            return Result.Failure<CallbackTokenContext>(new Error("CallbackTokenExpired",
                "Callback token is expired."));

        return Result.Success(new CallbackTokenContext(
            new ProvisioningSessionId(sessionGuid),
            new MachineId(machineGuid),
            expiresAtUtc
        ));
    }
}