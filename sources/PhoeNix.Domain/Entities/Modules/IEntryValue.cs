using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public interface IEntryValue
{
    public string Name { get; init; }

    public Guid Placeholder { get; init; }

    public abstract string Value { get; }
}