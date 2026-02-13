using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Modules;

namespace PhoeNix.Domain.Models.Systems;

public record SystemListResponse(
    SystemId Id,
    string Name,
    Architecture Architecture
);

public record SystemResponse(
    SystemId Id,
    string Name,
    Architecture Architecture,
    List<ModuleValueResponse> Modules
);