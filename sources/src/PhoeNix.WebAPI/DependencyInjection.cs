using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhoeNix.Application;
using PhoeNix.Domain.Options;
using PhoeNix.Infrastructure;
using PhoeNix.Persistence;
using Phoenix.Presentation.Extensions;
using System.Text;

namespace PhoeNix.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        services.AddCarter(configurator: c =>
        {
            c.WithValidatorsFromAssembly(typeof(PhoeNix.Application.DependencyInjection).Assembly);
        });

        services.AddWebApiOptions(configuration);
        services.AddProvisioningCallbackAuth(configuration);

        services.AddAuthorization();

        services.AddPersistence(configuration);
        services.AddInfrastructure();
        services.AddApplication();

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

        services.AddOptions<BootstrapArtifactsOptions>()
            .BindConfiguration("BootstrapArtifacts")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NetbootHostOptions>()
            .BindConfiguration("NetbootHost")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddProvisioningCallbackAuth(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection("CallbackToken").Get<JwtCallbackTokenOptions>()
                  ?? throw new InvalidOperationException("CallbackToken options are missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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

        return services;
    }
}
