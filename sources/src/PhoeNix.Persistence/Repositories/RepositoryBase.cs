using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

public abstract class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
    where TId : StronglyTypedId
    where TEntity : Entity<TId>
{
    protected readonly ApplicationDbContext DbContext;

    protected RepositoryBase(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public void Add(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
    }

    public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken token)
    {
        return await DbContext.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, token);
    }
}