using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public sealed record GetSetupSessionDetail(SetupSessionId SessionId) : IQuery<SetupSessionDetailResponse>;

internal sealed class GetSetupSessionDetailHandler(
    ISetupSessionRepository setupSessionRepository,
    IConfigurationReadRepository configurationReadRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetSetupSessionDetail, SetupSessionDetailResponse>
{
    public async Task<Result<SetupSessionDetailResponse>> Handle(GetSetupSessionDetail request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<SetupSessionDetailResponse>(userIdResult.Error);

        return await setupSessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(SetupSessionErrors.NotFound(request.SessionId))
            .Ensure(s => s.OwnerId == userIdResult.Value, SetupSessionErrors.NotFound(request.SessionId))
            .Bind(session => configurationReadRepository
                .GetByIdsAsync(session.Targets.Select(t => t.SelectedConfigurationId).ToList()!, cancellationToken)!
                .EnsureNotNull(ConfigurationErrors.NotFound())
                .Map(configurations => SetupSessionMappings.MapSetupSessionToDto(session, configurations)));
    }
}