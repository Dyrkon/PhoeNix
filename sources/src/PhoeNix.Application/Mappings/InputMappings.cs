using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Models.Inputs;

namespace PhoeNix.Application.Mappings;

public static class InputMappings
{
    public static InputResponse MapInputToDto(Input input)
    {
        return input.Follows != null ? 
            new InputResponse(input.Id, input.Source, input.Name, MapInputToDto(input.Follows)) :
            new InputResponse(input.Id, input.Source, input.Name);
    }
}