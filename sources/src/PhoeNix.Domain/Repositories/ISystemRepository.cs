using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Domain.Repositories;

public interface ISystemRepository : IRepository<Entities.Systems.System, SystemId>
{
    Task<Entities.Systems.System?> GetByNameAsync(string name, CancellationToken token);
}