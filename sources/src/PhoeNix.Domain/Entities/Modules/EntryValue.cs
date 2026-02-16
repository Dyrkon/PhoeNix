using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public abstract class EntryValue
{
    public EntryValueId Id { get; init; }

    public string Name { get; init; }

    public string Placeholder { get; init; }

    public string Value { get; protected set; }

    public ModuleValueId ModuleValueId { get; private set; }
}

public record EntryValueDefinition(
    ModuleTemplateId ModuleTemplateId,
    string Name,
    string Placeholder,
    UserInputType InputType);