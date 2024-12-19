using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Infrastructure.BackgroundJobs;
using Quartz;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(config => { config.RegisterServicesFromAssemblyContaining<AssemblyReference>(); });

        services.AddQuartzBackgroundJobs(configuration);
        services.AddQuartzHostedService();

        return services;
    }

    private static IServiceCollection AddQuartzBackgroundJobs(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddQuartz(configure =>
        {
            var processOutboxMessagesJobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

            configure
                .AddJob<ProcessOutboxMessagesJob>(processOutboxMessagesJobKey)
                .AddTrigger(trigger =>
                {
                    trigger.ForJob(processOutboxMessagesJobKey)
                        .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(10).RepeatForever());
                });
        });

        return services;
    }
}