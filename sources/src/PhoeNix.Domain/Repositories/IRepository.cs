using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Repositories;

public interface IRepository<TEntity, TId> 
    where TId : StronglyTypedId
    where TEntity : Entity<TId>
{
    void Add(TEntity entity);

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken token);
}