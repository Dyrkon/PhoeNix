namespace PhoeNix.Common.Models;

public record PagedRequest(
    int Page = 1,
    int PageSize = 10,
    string? OrderBy = null,
    string? Filter = null);