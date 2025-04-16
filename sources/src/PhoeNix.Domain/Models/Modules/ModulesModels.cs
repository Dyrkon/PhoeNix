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
    List<ModuleEntryResponse> ModuleEntries,
    List<Architecture> SupportedArchitectures
);

public record ModuleEntryResponse(
    ModuleEntryId Id,
    string Content,
    List<EntryValueResponse> EntryValues
);

public record EntryValueResponse(
    EntryValueId Id,
    string Name,
    Guid Placeholder,
    string Value
);