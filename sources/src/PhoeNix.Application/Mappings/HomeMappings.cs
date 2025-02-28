using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Models.Homes;

namespace PhoeNix.Application.Mappings;

public static class HomeMappings
{
    public static HomeListResponse MapHomeToListDto(Home home)
    {
        return new HomeListResponse(home.Id, home.Name, home.HomeUser.UserId);
    }

    public static HomeResponse MapHomeToDto(Home home)
    {
        return new HomeResponse(home.Id, home.Name, UserMappings.MapUserToDto(home.HomeUser.User),
            home.Modules.Select(h => ModuleMappings.MapModuleToListDto(h.Module)).ToList());
    }
}