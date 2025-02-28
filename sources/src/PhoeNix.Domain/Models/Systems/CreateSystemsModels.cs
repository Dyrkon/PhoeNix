using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Systems;

public record CreateSystemRequest
{
    public string Name { get; set; }

    public Architecture Architecture { get; set; }
}