using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Modules;

public record CreateModuleRequest
{
    private string Name { get; set; }

    private bool Enabled { get; set; }

    private ModuleType Type { get; set; }
}

public record CreateModuleEntryRequest
{
    private string Content { get; set; }
}

public record CreateEntryValueRequest
{
    private string Name { get; set; }
    private Guid Placeholder { get; set; }
    private string Value { get; set; }
}