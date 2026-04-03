namespace PhoeNix.Common.Models;

public abstract record ListRequestBase(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    SortDirection SortDirection = SortDirection.Ascending);