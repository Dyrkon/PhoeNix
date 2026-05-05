using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Options;
using PhoeNix.McpServer.Auth;
using PhoeNix.McpServer.Services;
using PhoeNix.Persistence;

namespace PhoeNix.McpServer;

public static class McpServerDependencyInjection
{
    public static IServiceCollection AddMcpServerOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FileStorageOptions>()
            .BindConfiguration("FileStorage");

        services.AddOptions<SshKeyStorageOptions>()
            .BindConfiguration("SshKeyFileStore");

        services.AddOptions<SshCaOptions>()
            .BindConfiguration("SshCa");

        services.AddOptions<JwtCallbackTokenOptions>()
            .BindConfiguration("CallbackToken");

        services.AddOptions<NetbootHostOptions>()
            .BindConfiguration("NetbootHost");

        services.AddOptions<HardwareProbeOptions>()
            .BindConfiguration("HardwareProbe");

        services.AddOptions<NixosInstallerOptions>()
            .BindConfiguration("NixosInstaller");

        services.AddOptions<NixOsUpdaterOptions>()
            .BindConfiguration("NixosUpdater");

        services.AddOptions<MonitoringOptions>()
            .BindConfiguration("Monitoring");

        return services;
    }

    public static IServiceCollection AddMcpHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSeeding();
        services.AddSingleton<McpAuthCodeStore>();
        services.AddSingleton<McpJwtService>();
        services.AddScoped<ICurrentUserAccessor, McpCurrentUserAccessor>();

        var signingKey = configuration["CallbackToken:SigningKey"]
                         ?? throw new InvalidOperationException("CallbackToken:SigningKey is required.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = McpJwtService.Issuer,
                    ValidateAudience = true,
                    ValidAudience = McpJwtService.Audience,
                    ValidateLifetime = true,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.Headers["WWW-Authenticate"] =
                            $"Bearer resource_metadata=\"{baseUrl}/.well-known/oauth-protected-resource\"";
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
