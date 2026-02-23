using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Modules;

public record ModuleTemplateListResponse(
    ModuleTemplateId Id,
    string Name,
    ModuleType Type
);

public record EntryValueDefinitionResponse(
    string Name,
    string Placeholder,
    UserInputType InputType
);

public record ModuleTemplateResponse(
    ModuleTemplateId Id,
    string Name,
    ModuleType Type,
    string Content,
    List<EntryValueDefinitionResponse> EditableValueTypes,
    List<Architecture> SupportedArchitectures
);

public record EntryValueResponse(
    string Name,
    string Placeholder,
    string Value
);

public record ModuleValueResponse(
    ModuleValueId Id,
    bool Enabled,
    List<EntryValueResponse> EditableValues
);

public record ModuleValueListResponse(
    ModuleValueId Id,
    bool Enabled
);