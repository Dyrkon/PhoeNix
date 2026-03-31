namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record SystemListResponse(
    Guid Id,
    string Name,
    Architecture Architecture);

public sealed record SystemResponse(
    Guid Id,
    string Name,
    Architecture Architecture,
    IReadOnlyList<ModuleValueResponse> Modules);

public sealed record SystemTestResponse(
    Guid Id,
    bool IsSuccess,
    string BuildTime);