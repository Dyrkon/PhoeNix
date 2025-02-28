using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Modules;

public record EditModuleRequest
{
    private string Name { get; set; }

    private bool Enabled { get; set; }

    private ModuleType Type { get; set; }
}

public record EditModuleEntryRequest
{
    private string Content { get; set; }
}

public record EditEntryValueRequest
{
    private string Name { get; set; }
    private Guid Placeholder { get; set; }
    private string Value { get; set; }
}