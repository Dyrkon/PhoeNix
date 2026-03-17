namespace PhoeNix.Application.Models.Setup;

public sealed record BootstrapCallbackRequest(Guid SessionId, Guid MachineId);