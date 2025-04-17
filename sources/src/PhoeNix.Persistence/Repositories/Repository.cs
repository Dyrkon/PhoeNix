using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Persistence.Repositories;

public abstract class Repository<TEntity, TId>
    where TId : StronglyTypedId
    where TEntity : Entity<TId>
{
    protected readonly ApplicationDbContext DbContext;

    protected Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public void Add(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
    }
    
    public virtual Task<TEntity?> GetByIdAsync(TId id)
    {
        return DbContext.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id);
    }
}