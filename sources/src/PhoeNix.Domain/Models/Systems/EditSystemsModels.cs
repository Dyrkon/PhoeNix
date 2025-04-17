using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Models.Systems;

public record EditSystemRequest
{
    public string Name { get; set; }

    public Architecture Architecture { get; set; }
}