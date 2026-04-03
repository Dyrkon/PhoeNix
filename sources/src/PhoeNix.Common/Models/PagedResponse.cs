namespace PhoeNix.Common.Models;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalItems);