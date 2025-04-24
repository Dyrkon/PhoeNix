using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Modules;

public record ModuleListResponse(
    ModuleId Id,
    string Name,
    bool Enabled,
    ModuleType Type
);

public record ModuleResponse(
    ModuleId Id,
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    List<EntryValueResponse> EntryValues,
    List<Architecture> SupportedArchitectures
);

public record EntryValueResponse(
    EntryValueId Id,
    string Name,
    string Placeholder,
    string Value
);