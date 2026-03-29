using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Systems;

public sealed record SystemListResponse(
    Guid Id,
    string Name,
    Architecture Architecture);

public sealed record SystemResponse(
    Guid Id,
    string Name,
    Architecture Architecture,
    IReadOnlyList<ModuleValueResponse> Modules);