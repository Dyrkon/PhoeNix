using PhoeNix.Domain.Entities.Flakes;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Homes;
using PhoeNix.Domain.Models.Inputs;
using PhoeNix.Domain.Models.Modules;
using PhoeNix.Domain.Models.Systems;

namespace PhoeNix.Domain.Models.Flakes;

public record FlakeListResponse(
    FlakeId Id,
    string Title,
    string Description
);

public record FlakeResponse(
    FlakeId Id,
    string Title,
    string Description,
    List<InputResponse> Inputs,
    List<ModuleListResponse> Modules,
    List<SystemListResponse> Systems,
    List<HomeListResponse> Homes,
    List<Architecture> SupportedArchitectures
);