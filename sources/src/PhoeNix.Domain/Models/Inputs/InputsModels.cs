using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Domain.Models.Inputs;

public record InputResponse(
    InputId Id,
    string Source,
    string Name,
    List<FollowInputResponse> Follows
);

public record FollowInputResponse(string FollowName, string FollowValue);