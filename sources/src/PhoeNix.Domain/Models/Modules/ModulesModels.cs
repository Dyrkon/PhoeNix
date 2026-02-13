using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Modules;

public record ModuleTemplateListResponse(
    ModuleTemplateId TemplateId,
    string Name,
    ModuleType Type
);

public record ModuleTemplateResponse(
    ModuleTemplateId TemplateId,
    string Name,
    ModuleType Type,
    string Content,
    List<EntryValueDefinitionResponse> EntryValues,
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
    List<EntryValueResponse> EntryValueDefinitions
);

public record ModuleValueListResponse(
    ModuleValueId Id,
    bool Enabled
);

public record EntryValueDefinitionResponse(
    string Name,
    string Placeholder,
    UserInputType InputType
);