using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Systems;

public record SystemListResponse(
    SystemId Id,
    Architecture Architecture,
    string Name,
    List<ModuleValueListResponse> Modules
);