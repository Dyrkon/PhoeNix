using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Inputs;
using PhoeNix.Domain.Models.Modules;
using PhoeNix.Domain.Models.Systems;

namespace PhoeNix.Domain.Models.Configurations;

public record ConfigurationListResponse(
    ConfigurationId Id,
    string Title,
    string Description
);

public record ConfigurationResponse(
    ConfigurationId Id,
    string Title,
    string Description,
    List<InputResponse> Inputs,
    List<ModuleValueResponse> Modules,
    List<SystemListResponse> Systems,
    List<Architecture> SupportedArchitectures
);