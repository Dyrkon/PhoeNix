using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Modules;

namespace PhoeNix.Domain.Models.Systems;

public record SystemListResponse(
    SystemId Id,
    Architecture Architecture,
    string Name,
    List<ModuleValueListResponse> Modules
);