using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.Repositories;

public interface ITestRepository : IRepository<Test, TestId>
{
    Task<Test?> GetByNameAsync(string name, CancellationToken token);
}