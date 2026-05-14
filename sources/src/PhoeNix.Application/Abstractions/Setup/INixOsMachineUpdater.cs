using System.Net;
using PhoeNix.Application.Models.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface INixOsMachineUpdater
{
    Task<Result<ProcessResult>> UpdateAsync(
        IPAddress targetIpAddress,
        string targetHostname,
        string flakeDirectory,
        string systemAttribute,
        DeploySshAccessMaterial deployIdentity,
        CancellationToken cancellationToken);
}