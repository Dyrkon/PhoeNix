using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Options;

namespace PhoeNix.Infrastructure.Services.Monitoring;

public sealed class PrometheusTokenService : IPrometheusTokenService
{
    private const string PrometheusAudience = "PhoeNix.Prometheus";

    private readonly JsonWebTokenHandler _handler = new();
    private readonly JwtCallbackTokenOptions _jwtOptions;
    private readonly MonitoringOptions _monitoringOptions;
    private readonly TokenValidationParameters _validationParameters;

    public PrometheusTokenService(
        IOptions<JwtCallbackTokenOptions> jwtOptions,
        IOptions<MonitoringOptions> monitoringOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _monitoringOptions = monitoringOptions.Value;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = PrometheusAudience,
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = _jwtOptions.AllowedClockSkew
        };
    }

    public string CreateToken()
    {
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = PrometheusAudience,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.Add(_monitoringOptions.TokenTtl),
            Subject = new ClaimsIdentity(),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return _handler.CreateToken(descriptor);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var result = await _handler.ValidateTokenAsync(token, _validationParameters);
        return result.IsValid;
    }
}
