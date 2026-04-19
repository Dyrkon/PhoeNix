using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public record GetSetupMachineStatusQuery(SetupSessionId SessionId, MachineId MachineId) : IQuery<SetupStatusResponse>;

internal sealed class GetMachineStatusQueryHandler(
    ISetupSessionRepository setupSessionRepository)
    : IQueryHandler<GetSetupMachineStatusQuery, SetupStatusResponse>
{
    public Task<Result<SetupStatusResponse>> Handle(
        GetSetupMachineStatusQuery request,
        CancellationToken cancellationToken)
    {
        return setupSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error("SetupSessionNotFound", $"Setup session '{request.SessionId.Value}' was not found."))
            .Bind(session => session.Targets
                .FirstOrDefault(t => t.MachineId == request.MachineId)
                .EnsureNotNull(new Error("MachineNotInSession", $"Machine '{request.MachineId.Value}' is not enrolled in session '{request.SessionId.Value}'.")))
            .Map(target =>
            {
                var lastError = target.LastErrorCode is null || target.LastErrorDescription is null || target.LastErrorAtUtc is null
                    ? null
                    : new SetupErrorSnapshotResponse(
                        target.LastErrorCode,
                        target.LastErrorDescription,
                        target.LastErrorSource ?? "unknown",
                        target.LastErrorAtUtc.Value);

                return new SetupStatusResponse(target.Stage, target.LastTransitionAtUtc, lastError);
            });
    }
}