using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Persistence.Repositories;

public class SetupSessionRepository : RepositoryBase<SetupSession, SetupSessionId>,
    ISetupSessionRepository
{
    public SetupSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SetupSession?> GetWithEnrolledMachineAsync(MachineId machineId,
        CancellationToken cancellationToken)
    {
        return DbContext
            .Set<SetupSession>()
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(
                p => p.Targets.Any(t => t.MachineId == machineId),
                cancellationToken);
    }

    public async Task<PagedResponse<SetupSession>> GetSetupSessions(SetupSessionsRequest sessionsRequest,
        CancellationToken cancellationToken)
    {
        return await GetPageAsync(sessionsRequest, cancellationToken);
    }

    public async Task<PagedResponse<SetupSession>> GetPageAsync(
        SetupSessionsRequest request,
        CancellationToken cancellationToken)
    {
        var query = DbContext.SetupSessions
            .AsNoTracking();

        query = ApplySorting(query, request);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<SetupSession>(items, totalItems);
    }

    private static IQueryable<SetupSession> ApplySorting(
        IQueryable<SetupSession> query,
        SetupSessionsRequest request)
    {
        return request.SortDirection switch
        {
            SortDirection.Ascending => query.OrderBy(s =>
                s.StartTime),
            SortDirection.Descending => query.OrderByDescending(s =>
                s.StartTime),

            _ => query.OrderBy(s => s.StartTime)
        };
    }
}