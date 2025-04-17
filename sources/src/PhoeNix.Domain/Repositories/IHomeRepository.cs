using PhoeNix.Domain.Entities.Homes;

namespace PhoeNix.Domain.Repositories;

public interface IHomeRepository : IRepository<Home, HomeId>
{
    Task<Home?> GetByNameAsync(string name, CancellationToken token);
}