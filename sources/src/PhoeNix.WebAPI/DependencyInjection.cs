using Carter;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Options;
using PhoeNix.Infrastructure;
using PhoeNix.Persistence;
using System.Text;
using Phoenix.Presentation.Extensions;

namespace PhoeNix.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        services.AddCors(options =>
        {
            options.AddPolicy("WebAppClient", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5269",
                        "https://localhost:7052",
                        "http://localhost:8888")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddCarter(configurator: c =>
        {
            c.WithValidatorsFromAssembly(typeof(PhoeNix.Application.DependencyInjection).Assembly);
        });

        services.AddWebApiOptions(configuration);
        services.AddPhoeNixAuthentication(configuration);

        services.AddPersistence(configuration);
        services.AddInfrastructure();
        services.AddApplication();
        services.AddSeeding();

        return services;
    }

    private static IServiceCollection AddWebApiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FileStorageOptions>()
            .BindConfiguration("FileStorage")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SshKeyStorageOptions>()
            .BindConfiguration("SshKeyFileStore")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SshCaOptions>()
            .BindConfiguration("SshCa")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtCallbackTokenOptions>()
            .BindConfiguration("CallbackToken")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "CallbackToken:SigningKey is required.")
            .ValidateOnStart();

        services.AddOptions<NetbootHostOptions>()
            .BindConfiguration("NetbootHost")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<HardwareProbeOptions>()
            .BindConfiguration("HardwareProbe")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NixosInstallerOptions>()
            .BindConfiguration("NixosInstaller")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SeedExampleOptions>()
            .BindConfiguration("SeedExample")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NixOsUpdaterOptions>()
            .BindConfiguration("NixosUpdater")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MonitoringOptions>()
            .BindConfiguration("Monitoring");

        return services;
    }

    private static IServiceCollection AddPhoeNixAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        services.AddHttpContextAccessor();

        var jwt = configuration.GetSection("CallbackToken").Get<JwtCallbackTokenOptions>()
                  ?? throw new InvalidOperationException("CallbackToken options are missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationSchemeNames.UserCookie;
                options.DefaultChallengeScheme = AuthenticationSchemeNames.UserCookie;
                options.DefaultSignInScheme = AuthenticationSchemeNames.UserCookie;
            })
            .AddCookie(AuthenticationSchemeNames.UserCookie, options =>
            {
                options.Cookie.Name = "phoenix.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.None;
                options.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateLifetime = true,
                    ClockSkew = jwt.AllowedClockSkew,

                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(AuthenticationSchemeNames.UserCookie)
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("ProvisioningCallback", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseUserAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}