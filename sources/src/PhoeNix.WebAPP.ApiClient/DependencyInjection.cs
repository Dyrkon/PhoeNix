using Microsoft.Extensions.DependencyInjection;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Clients;

namespace PhoeNix.WebAPP.ApiClient;

public static class DependencyInjection
{
    public static IServiceCollection AddPhoeNixApiClients(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationInvalidationNotifier, AuthenticationInvalidationNotifier>();
        services.AddScoped<IAuthenticationApiClient, AuthenticationApiClient>();
        services.AddScoped<IConfigurationsApiClient, ConfigurationsApiClient>();
        services.AddScoped<IModulesApiClient, ModulesApiClient>();
        services.AddScoped<IMachinesApiClient, MachinesApiClient>();
        services.AddScoped<ISystemsApiClient, SystemsApiClient>();
        services.AddScoped<ISetupApiClient, SetupApiClient>();
        services.AddScoped<IDeploymentApiClient, DeploymentApiClient>();
        services.AddScoped<IMetricsApiClient, MetricsApiClient>();
        services.AddScoped<ISettingsApiClient, SettingsApiClient>();
        services.AddScoped<IGitOpsApiClient, GitOpsApiClient>();

        return services;
    }
}