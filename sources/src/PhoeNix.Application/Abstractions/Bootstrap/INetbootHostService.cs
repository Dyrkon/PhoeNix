using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface INetbootHostService
{
    Task<Result> StartAsync(CancellationToken cancellationToken);

    Task<Result> StopAsync(CancellationToken cancellationToken);

    Task<Result<NetbootHostStatus>> GetStatusAsync(CancellationToken cancellationToken);
}