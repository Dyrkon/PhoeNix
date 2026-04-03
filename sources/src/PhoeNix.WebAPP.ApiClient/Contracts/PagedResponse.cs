namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalItems);