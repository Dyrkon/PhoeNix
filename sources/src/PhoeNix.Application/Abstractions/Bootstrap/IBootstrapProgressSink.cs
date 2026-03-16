using PhoeNix.Application.Models.Setup;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface IBootstrapProgressSink
{
    Task ReportAsync(BootstrapBuildProgress progress, CancellationToken cancellationToken);
}