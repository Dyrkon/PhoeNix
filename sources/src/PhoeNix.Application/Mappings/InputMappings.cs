using PhoeNix.Application.Models.Inputs;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.Mappings;

public static class InputMappings
{
    public static InputResponse MapInputToDto(Input input)
    {
        return new InputResponse(
            input.Id.Value,
            input.Source,
            input.Name,
            input.Followers
                .Select(MapInputFollowToDto)
                .ToList());
    }

    public static InputFollowResponse MapInputFollowToDto(FollowInput follow)
    {
        return new InputFollowResponse(
            follow.Id,
            follow.FollowName,
            follow.FollowValue);
    }

    public static InputFollowDraft MapInputFollowToDomain(InputFollowUpsertModel model)
    {
        return new InputFollowDraft(model.FollowName, model.FollowValue);
    }
}