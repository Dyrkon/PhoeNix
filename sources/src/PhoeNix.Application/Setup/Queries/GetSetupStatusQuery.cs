using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public record GetSetupStatusQuery(SetupSessionId SessionId, MachineId MachineId) : IQuery<SetupStatusResponse>;

internal sealed class GetMachineStatusQueryHandler(
    ISetupSessionRepository setupSessionRepository)
    : IQueryHandler<GetSetupStatusQuery, SetupStatusResponse>
{
    public async Task<Result<SetupStatusResponse>> Handle(
        GetSetupStatusQuery request,
        CancellationToken cancellationToken)
    {
        var session = await setupSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure<SetupStatusResponse>(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == request.MachineId);

        if (target is null)
            return Result.Failure<SetupStatusResponse>(new Error(
                "MachineNotInSession",
                $"Machine '{request.MachineId.Value}' is not enrolled in session '{request.SessionId.Value}'."));

        var lastError = target.LastErrorCode is null || target.LastErrorDescription is null ||
                        target.LastErrorAtUtc is null
            ? null
            : new SetupErrorSnapshotResponse(
                target.LastErrorCode,
                target.LastErrorDescription,
                target.LastErrorSource ?? "unknown",
                target.LastErrorAtUtc.Value);

        return Result.Success(new SetupStatusResponse(
            target.Stage,
            target.LastTransitionAtUtc,
            lastError));
    }
}