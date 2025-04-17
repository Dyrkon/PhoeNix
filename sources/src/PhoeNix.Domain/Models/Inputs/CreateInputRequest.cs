using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Domain.Models.Inputs;

public record CreateInputRequest
{
    public string Source { get; set; }

    public string Name { get; set; }

    public InputId? InputId { get; set; }
}