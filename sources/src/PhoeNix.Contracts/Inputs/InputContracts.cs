namespace PhoeNix.Contracts.Inputs;

public sealed record InputFollowUpsertModel(string? FollowName, string? FollowValue);

public sealed record InputFollowResponse(Guid Id, string FollowName, string FollowValue);

public sealed record InputResponse(
    Guid Id,
    string Source,
    string Name,
    IReadOnlyList<InputFollowResponse> Followers);
