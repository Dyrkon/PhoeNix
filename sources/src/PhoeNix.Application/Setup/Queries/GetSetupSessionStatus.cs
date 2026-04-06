using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public record GetSetupSessionStatus(SetupSessionId SessionId) : IQuery<SetupSessionListResponse>;

internal sealed class GetSetupSessionStatusHandler(ISetupSessionRepository sessionRepository)
    : IQueryHandler<GetSetupSessionStatus, SetupSessionListResponse>
{
    public async Task<Result<SetupSessionListResponse>> Handle(GetSetupSessionStatus request,
        CancellationToken cancellationToken)
    {
        return await sessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(SetupSessionErrors.NotFound(request.SessionId))
            .Map(SetupSessionMappings.MapSetupSessionToListDto);
    }
}