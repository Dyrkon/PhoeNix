using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public interface IEntryValue
{
    public EntryValueId Id { get; init; }

    public string Name { get; init; }

    public Guid Placeholder { get; init; }

    public string Value { get; }
}