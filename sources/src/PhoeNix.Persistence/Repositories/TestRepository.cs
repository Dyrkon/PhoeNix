using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class TestRepository : RepositoryBase<Test, TestId>, ITestRepository
{
    public TestRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Test?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Tests.SingleOrDefaultAsync(t => t.Name.Contains(name), token);
    }
}