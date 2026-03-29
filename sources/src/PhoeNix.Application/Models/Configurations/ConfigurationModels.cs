using PhoeNix.Application.Models.Inputs;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Configurations;

public sealed record ConfigurationListResponse(
    Guid Id,
    string Title,
    string Description);

public sealed record ConfigurationResponse(
    Guid Id,
    string Title,
    string Description,
    IReadOnlyList<InputResponse> Inputs,
    IReadOnlyList<ModuleValueResponse> Modules,
    IReadOnlyList<SystemListResponse> Systems,
    IReadOnlyList<Architecture> SupportedArchitectures);