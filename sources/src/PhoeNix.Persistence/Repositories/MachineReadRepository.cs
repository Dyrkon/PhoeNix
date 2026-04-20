using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Mappings;
using PhoeNix.Contracts.Machines;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Persistence.Repositories;

public sealed class MachineReadRepository(
    ApplicationDbContext dbContext,
    IMachineRepository machineRepository) : IMachineReadRepository
{
    public async Task<PagedResponse<MachineListResponse>> GetPageAsync(
        ListMachinesRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Machines
            .AsNoTracking();

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(machine => new MachineListResponse(
                machine.Id.Value,
                machine.DeploymentSnapshot != null ? machine.DeploymentSnapshot.ConfigurationId.Value : null,
                machine.Title,
                machine.Enabled,
                machine.MacAddress.ToString(),
                machine.Architecture,
                machine.MachineStatus.MachineState))
            .ToListAsync(cancellationToken);

        return new PagedResponse<MachineListResponse>(items, totalItems);
    }

    public async Task<MachineDetailResponse?> GetByIdAsync(
        Guid machineId,
        CancellationToken cancellationToken)
    {
        var machine = await machineRepository.GetByIdAsync(new MachineId(machineId), cancellationToken);

        return machine is null
            ? null
            : MachineMapping.MapMachineToDto(machine);
    }

    private static IQueryable<Machine> ApplyFilters(
        IQueryable<Machine> query,
        ListMachinesRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search?.Trim().ToLower();

            query = query.Where(machine =>
                machine.Title.ToLower().Contains(search ?? ""));
        }

        if (request.Enabled.HasValue)
            query = query.Where(machine => machine.Enabled == request.Enabled.Value);

        if (request.Architecture.HasValue)
            query = query.Where(machine => machine.Architecture == request.Architecture.Value);

        if (request.MachineState.HasValue)
            query = query.Where(machine => machine.MachineStatus.MachineState == request.MachineState.Value);

        return query;
    }

    private static IQueryable<Machine> ApplySorting(
        IQueryable<Machine> query,
        ListMachinesRequest request)
    {
        return (request.SortField, request.SortDirection) switch
        {
            (MachineSortField.Title, SortDirection.Ascending) => query.OrderBy(machine => machine.Title),
            (MachineSortField.Title, SortDirection.Descending) => query.OrderByDescending(machine => machine.Title),

            (MachineSortField.MacAddress, SortDirection.Ascending) => query.OrderBy(machine => machine.MacAddress),
            (MachineSortField.MacAddress, SortDirection.Descending) => query.OrderByDescending(machine =>
                machine.MacAddress),

            (MachineSortField.Architecture, SortDirection.Ascending) => query.OrderBy(machine => machine.Architecture),
            (MachineSortField.Architecture, SortDirection.Descending) => query.OrderByDescending(machine =>
                machine.Architecture),

            (MachineSortField.MachineState, SortDirection.Ascending) => query.OrderBy(machine =>
                machine.MachineStatus.MachineState),
            (MachineSortField.MachineState, SortDirection.Descending) => query.OrderByDescending(machine =>
                machine.MachineStatus.MachineState),

            (MachineSortField.Enabled, SortDirection.Ascending) => query.OrderBy(machine => machine.Enabled),
            (MachineSortField.Enabled, SortDirection.Descending) => query.OrderByDescending(machine => machine.Enabled),

            _ => query.OrderBy(machine => machine.Title)
        };
    }
}