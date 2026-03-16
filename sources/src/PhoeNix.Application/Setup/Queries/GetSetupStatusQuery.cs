using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public record GetSetupStatusQuery(SetupSessionId SessionId, MachineId MachineId) : IQuery<SetupStage>;

internal sealed class GetMachineStatusQueryHandler(
    ISetupSessionRepository setupSessionRepository)
    : IQueryHandler<GetSetupStatusQuery, SetupStage>
{
    public async Task<Result<SetupStage>> Handle(
        GetSetupStatusQuery request,
        CancellationToken cancellationToken)
    {
        var session = await setupSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure<SetupStage>(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == request.MachineId);

        if (target is null)
            return Result.Failure<SetupStage>(new Error(
                "MachineNotInSession",
                $"Machine '{request.MachineId.Value}' is not enrolled in session '{request.SessionId.Value}'."));

        return Result.Success(target.Stage);
    }
}