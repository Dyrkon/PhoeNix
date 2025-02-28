namespace PhoeNix.Domain.Models.Flakes;

public record CreateFlakeRequest
{
    public string Name { get; set; }

    public string Description { get; set; }
}