using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Models.Inputs;

namespace PhoeNix.Application.Mappings;

public static class InputMappings
{
    public static InputResponse MapInputToDto(Input input)
    {
        return input.Followers.Any()
            ? new InputResponse(input.Id, input.Source, input.Name,
                MapInputsFollowsToDto(input.Followers.ToList()))
            : new InputResponse(input.Id, input.Source, input.Name, []);
    }

    public static List<FollowInputResponse> MapInputsFollowsToDto(List<FollowInput> followInputs)
    {
        return followInputs.Select(fI => new FollowInputResponse(fI.FollowName, fI.FollowValue)).ToList();
    }
}