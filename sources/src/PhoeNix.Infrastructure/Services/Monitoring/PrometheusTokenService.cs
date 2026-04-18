using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Options;
using PhoeNix.Application.Repositories;

namespace PhoeNix.Infrastructure.Services.Monitoring;

public sealed class PrometheusTokenService : IPrometheusTokenService
{
    private const string PrometheusAudience = "PhoeNix.Prometheus";

    private readonly JsonWebTokenHandler _handler = new();
    private readonly JwtCallbackTokenOptions _jwtOptions;
    private readonly TokenValidationParameters _validationParameters;
    private readonly IServiceScopeFactory _scopeFactory;

    public PrometheusTokenService(
        IOptions<JwtCallbackTokenOptions> jwtOptions,
        IServiceScopeFactory scopeFactory)
    {
        _jwtOptions = jwtOptions.Value;
        _scopeFactory = scopeFactory;

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
        TimeSpan tokenTtl;
        using (var scope = _scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
            var settings = repo.GetAsync().GetAwaiter().GetResult();
            tokenTtl = settings is not null
                ? TimeSpan.FromDays(settings.MonitoringTokenTtlDays)
                : TimeSpan.FromDays(7);
        }

        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = PrometheusAudience,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.Add(tokenTtl),
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
