using PhoeNix.Application.Models.Inputs;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.Mappings;

public static class InputMappings
{
    public static InputResponse MapInputToDto(Input input)
    {
        return new InputResponse(
            input.Id,
            input.Source,
            input.Name,
            MapInputsFollowsToDto(input.Followers.ToList()));
    }

    public static List<FollowInputResponse> MapInputsFollowsToDto(List<FollowInput> followInputs)
    {
        return followInputs
            .Select(f => new FollowInputResponse(f.FollowName, f.FollowValue))
            .ToList();
    }
}