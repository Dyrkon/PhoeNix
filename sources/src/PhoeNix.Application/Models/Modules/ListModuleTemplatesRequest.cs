using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Modules;

public sealed record ListModuleTemplatesRequest(
    ModuleTemplateSortField SortField = ModuleTemplateSortField.Name,
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    bool? Enabled = null,
    ModuleType? Type = null,
    SortDirection SortDirection = SortDirection.Ascending);

public enum ModuleTemplateSortField
{
    Name = 0,
    Type = 1,
    Enabled = 2
}
