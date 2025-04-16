using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Models.Modules;
using PhoeNix.Domain.Models.Users;

namespace PhoeNix.Domain.Models.Homes;

public record HomeListResponse(
    HomeId Id,
    string Name,
    UserId UserId
);

public record HomeResponse(
    HomeId Id,
    string Name,
    UserResponse User,
    List<ModuleListResponse> Modules
);