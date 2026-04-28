using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public enum MetricsTimeRange
{
    OneHour,
    SixHours,
    TwentyFourHours,
    SevenDays,
    ThirtyDays
}

public record MachineMetricsResult(
    bool IsUp,
    string? Uptime,
    double DiskSpaceUsed,
    PrometheusRangeSeries Cpu,
    PrometheusRangeSeries Ram,
    PrometheusRangeSeries NetRx,
    PrometheusRangeSeries NetTx,
    PrometheusRangeSeries DiskRead,
    PrometheusRangeSeries DiskWrite
);

public sealed record GetMachineMetricsQuery(MachineId MachineId, MetricsTimeRange Range)
    : IQuery<MachineMetricsResult>;

internal sealed class GetMachineMetricsQueryHandler(
    IMachineReadRepository machineReadRepository,
    IPrometheusQueryClient prometheusQueryClient)
    : IQueryHandler<GetMachineMetricsQuery, MachineMetricsResult>
{
    public Task<Result<MachineMetricsResult>> Handle(
        GetMachineMetricsQuery request,
        CancellationToken cancellationToken)
    {
        return machineReadRepository
            .GetByIdAsync(request.MachineId.Value, cancellationToken)
            .EnsureNotNull(MachineErrors.NotFound(request.MachineId))
            .Bind(machine => HandleAsync(machine.Title, request.Range, cancellationToken));
    }

    private async Task<Result<MachineMetricsResult>> HandleAsync(
        string title,
        MetricsTimeRange range,
        CancellationToken cancellationToken)
    {
        var (start, end, step) = ResolveTimeWindow(range);

        var upTask = prometheusQueryClient.QueryInstantAsync(
            $"up{{job=\"orchestrated-machines\",machine=\"{title}\"}}",
            cancellationToken);

        var uptimeTask = prometheusQueryClient.QueryInstantAsync(
            $"node_time_seconds{{machine=\"{title}\"}} - node_boot_time_seconds{{machine=\"{title}\"}}",
            cancellationToken);

        var diskSpaceUsed = prometheusQueryClient.QueryInstantAsync(
            $"100 - ((node_filesystem_avail_bytes{{mountpoint=\"/\",fstype!=\"rootfs\",machine=\"{title}\"}} * 100)/node_filesystem_size_bytes{{mountpoint=\"/\",fstype!=\"rootfs\",machine=\"{title}\"}})",
            cancellationToken);

        var cpuTask = prometheusQueryClient.QueryRangeAsync(
            $"100 - (avg by(machine)(irate(node_cpu_seconds_total{{mode=\"idle\",machine=\"{title}\"}}[5m])) * 100)",
            start, end, step, cancellationToken);

        var ramTask = prometheusQueryClient.QueryRangeAsync(
            $"(node_memory_MemTotal_bytes{{machine=\"{title}\"}} - node_memory_MemAvailable_bytes{{machine=\"{title}\"}}) / node_memory_MemTotal_bytes{{machine=\"{title}\"}} * 100",
            start, end, step, cancellationToken);

        var netRxTask = prometheusQueryClient.QueryRangeAsync(
            $"sum(irate(node_network_receive_bytes_total{{machine=\"{title}\",device!=\"lo\"}}[5m]))",
            start, end, step, cancellationToken);

        var netTxTask = prometheusQueryClient.QueryRangeAsync(
            $"sum(irate(node_network_transmit_bytes_total{{machine=\"{title}\",device!=\"lo\"}}[5m]))",
            start, end, step, cancellationToken);

        var diskReadTask = prometheusQueryClient.QueryRangeAsync(
            $"sum(irate(node_disk_read_bytes_total{{machine=\"{title}\"}}[5m]))",
            start, end, step, cancellationToken);

        var diskWriteTask = prometheusQueryClient.QueryRangeAsync(
            $"sum(irate(node_disk_written_bytes_total{{machine=\"{title}\"}}[5m]))",
            start, end, step, cancellationToken);

        await Task.WhenAll(upTask, uptimeTask, diskSpaceUsed, cpuTask, ramTask, netRxTask, netTxTask, diskReadTask,
            diskWriteTask);

        var isUp = await upTask is > 0.5;
        var uptimeSeconds = await uptimeTask;
        var uptimeFormatted = uptimeSeconds.HasValue ? FormatUptime(uptimeSeconds.Value) : null;
        var diskSpace = await diskSpaceUsed;
        var diskSpaceUsage = diskSpace ?? -1;

        return Result.Success(new MachineMetricsResult(
            isUp,
            uptimeFormatted,
            diskSpaceUsage,
            await cpuTask,
            await ramTask,
            await netRxTask,
            await netTxTask,
            await diskReadTask,
            await diskWriteTask
        ));
    }

    private static (DateTimeOffset Start, DateTimeOffset End, TimeSpan Step) ResolveTimeWindow(MetricsTimeRange range)
    {
        var end = DateTimeOffset.UtcNow;
        var (duration, step) = range switch
        {
            MetricsTimeRange.OneHour => (TimeSpan.FromHours(1), TimeSpan.FromSeconds(12)),
            MetricsTimeRange.SixHours => (TimeSpan.FromHours(6), TimeSpan.FromMinutes(1)),
            MetricsTimeRange.TwentyFourHours => (TimeSpan.FromHours(24), TimeSpan.FromMinutes(5)),
            MetricsTimeRange.SevenDays => (TimeSpan.FromDays(7), TimeSpan.FromMinutes(30)),
            MetricsTimeRange.ThirtyDays => (TimeSpan.FromDays(30), TimeSpan.FromHours(2)),
            _ => (TimeSpan.FromHours(24), TimeSpan.FromMinutes(5))
        };
        return (end - duration, end, step);
    }

    private static string FormatUptime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h";
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{ts.Minutes}m";
    }
}