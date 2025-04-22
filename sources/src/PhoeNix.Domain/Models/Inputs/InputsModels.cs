using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Domain.Models.Inputs;

public record InputResponse(
    InputId Id,
    string Source,
    string Name,
    InputResponse? Follows = null
);