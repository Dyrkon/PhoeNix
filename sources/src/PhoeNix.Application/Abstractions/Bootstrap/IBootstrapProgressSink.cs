using PhoeNix.Application.Models.Bootstrap;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface IBootstrapProgressSink
{
    Task ReportAsync(BootstrapBuildProgress progress, CancellationToken cancellationToken);
}
