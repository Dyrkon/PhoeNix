using PhoeNix.Application.Models.Inputs;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Configurations;

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