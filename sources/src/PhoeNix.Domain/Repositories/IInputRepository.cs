using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Domain.Repositories;

public interface IInputRepository : IRepository<Input, InputId>
{
    Task<Input?> GetByNameAsync(string name, CancellationToken token);
    Task<bool> IsSourceUniqueAsync(string source, CancellationToken token);
}