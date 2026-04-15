using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Options;

namespace PhoeNix.Infrastructure.Services.Monitoring;

internal sealed class PrometheusTokenWriterService(
    IPrometheusTokenService tokenService,
    IOptions<MonitoringOptions> options,
    ILogger<PrometheusTokenWriterService> logger)
    : BackgroundService
{
    private const string TokenFileName = "prometheus-token";

    private readonly MonitoringOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stateDir = ResolveStateDir();

        logger.LogInformation("Writing Prometheus token to {StateDir}.", stateDir);
        await WriteTokenAsync(stateDir, stoppingToken);

        var lastWrittenAt = DateTime.UtcNow;
        var refreshThreshold = _options.TokenTtl - TimeSpan.FromDays(1);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
            try
            {
                if (DateTime.UtcNow - lastWrittenAt >= refreshThreshold)
                {
                    await WriteTokenAsync(stateDir, stoppingToken);
                    lastWrittenAt = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to refresh Prometheus token.");
            }
    }

    private async Task WriteTokenAsync(string stateDir, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(stateDir);

        var token = tokenService.CreateToken();
        var tokenPath = Path.Combine(stateDir, TokenFileName);
        await File.WriteAllTextAsync(tokenPath, token, cancellationToken);

        logger.LogInformation("Prometheus token written to {TokenPath}.", tokenPath);
    }

    private string ResolveStateDir()
    {
        var dir = _options.StateDir
                  ?? Environment.GetEnvironmentVariable("PHOENIX_STATE_DIR")
                  ?? "/var/lib/phoenix";

        return Path.GetFullPath(dir);
    }
}