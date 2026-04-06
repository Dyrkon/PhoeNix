using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Mappings;

public static class SetupSessionMappings
{
    public static SetupSessionListResponse MapSetupSessionToListDto(SetupSession session)
    {
        var lastTransition = session.Targets
            .Select(t => t.LastTransitionAtUtc)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .OrderByDescending(t => t)
            .FirstOrDefault();

        var done = session.Targets.Count(t =>
            t.Stage is SetupStage.Finished or SetupStage.Cancelled);

        var failed = session.Targets.Count(t => t.Stage is SetupStage.Failed);

        return new SetupSessionListResponse(
            session.Id.Value,
            session.StartTime,
            lastTransition == default ? null : lastTransition,
            session.Targets.Count,
            done,
            failed);
    }

    public static SetupSessionDetailResponse MapSetupSessionToDto(
        SetupSession session,
        IReadOnlyList<Configuration> configurations)
    {
        var configurationsById = configurations.ToDictionary(c => c.Id);

        return new SetupSessionDetailResponse(
            session.Id.Value,
            session.StartTime,
            session.Targets
                .Select(t => t.LastTransitionAtUtc)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .OrderByDescending(t => t)
                .Cast<DateTime?>()
                .FirstOrDefault(),
            session.SshCredential?.ExpiresAtUtc,
            session.Targets
                .Select(t => MapSetupTargetToDto(t, configurationsById))
                .ToList());
    }

    private static SetupTargetResponse MapSetupTargetToDto(
        SetupTarget target,
        IReadOnlyDictionary<ConfigurationId, Configuration> configurationsById)
    {
        Configuration? configuration = null;
        if (target.SelectedConfigurationId is not null)
            configurationsById.TryGetValue(target.SelectedConfigurationId, out configuration);

        var systemName = configuration is not null && target.SelectedSystemId is not null
            ? configuration.SystemSpecifications
                .FirstOrDefault(s => s.Id == target.SelectedSystemId)?.Name
            : null;

        return new SetupTargetResponse(
            target.MachineId.Value,
            target.Stage,
            target.LastTransitionAtUtc,
            target.LastErrorCode,
            target.LastErrorDescription,
            target.LastErrorSource,
            target.LastErrorAtUtc,
            target.IpAddress?.ToString(),
            target.SelectedSystemId?.Value,
            systemName,
            target.SelectedConfigurationId?.Value,
            configuration?.Title,
            target.RankedDiskAssignments
                .Select(d => new RankedDiskAssignmentResponse(d.Index, d.DiskByIdPath))
                .ToList());
    }
}