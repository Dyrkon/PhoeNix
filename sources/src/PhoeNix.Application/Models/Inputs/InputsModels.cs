namespace PhoeNix.Application.Models.Inputs;

public sealed record InputFollowResponse(
    Guid Id,
    string FollowName,
    string FollowValue);

public sealed record InputResponse(
    Guid Id,
    string Source,
    string Name,
    IReadOnlyList<InputFollowResponse> Followers);

public sealed record InputFollowUpsertModel(
    string FollowName,
    string FollowValue);