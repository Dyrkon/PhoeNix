namespace PhoeNix.Domain.Models.Machines;

public record CreateMachineRequest(string Title, bool Enabled, string MacAddress);