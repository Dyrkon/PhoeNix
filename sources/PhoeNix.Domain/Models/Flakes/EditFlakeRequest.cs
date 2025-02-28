namespace PhoeNix.Domain.Models.Flakes;

public record EditFlakeRequest
{
    public string Name { get; set; }

    public string Description { get; set; }
}