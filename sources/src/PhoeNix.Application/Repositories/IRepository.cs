using Microsoft.EntityFrameworkCore.ChangeTracking;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Application.Repositories;

public interface IRepository<TEntity, TId>
    where TId : StronglyTypedId
    where TEntity : Entity<TId>
{
    void Add(TEntity entity);

    public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken);

    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken token);
}