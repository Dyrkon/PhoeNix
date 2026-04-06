using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public record GetSetupSessions(SetupSessionsRequest Request) : IQuery<PagedResponse<SetupSessionListResponse>>;

internal sealed class GetSetupSessionsHandler(ISetupSessionRepository sessionRepository)
    : IQueryHandler<GetSetupSessions, PagedResponse<SetupSessionListResponse>>
{
    public async Task<Result<PagedResponse<SetupSessionListResponse>>> Handle(GetSetupSessions request,
        CancellationToken cancellationToken)
    {
        return await sessionRepository
            .GetSetupSessions(request.Request, cancellationToken)
            .EnsurePagedNotEmpty(SetupSessionErrors.NoSessionAvailable())
            .Map(sessions => new PagedResponse<SetupSessionListResponse>(
                sessions.Items
                    .Select(SetupSessionMappings.MapSetupSessionToListDto)
                    .ToList(), sessions.TotalItems));
    }
}