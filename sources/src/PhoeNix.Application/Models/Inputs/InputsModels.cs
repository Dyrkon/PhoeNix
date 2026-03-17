using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.Models.Inputs;

public record FollowInputResponse(
    string FollowName,
    string FollowValue
);

public record InputResponse(
    InputId Id,
    string Source,
    string Name,
    List<FollowInputResponse> Follows
);