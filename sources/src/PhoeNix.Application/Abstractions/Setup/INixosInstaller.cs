using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface INixosInstaller
{
    Task<Result> InstallAsync(
        SetupSession session,
        SetupTarget target,
        string configurationDirectoryPath,
        string configurationName,
        CancellationToken cancellationToken);
}