namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record StartMachineSetupRequest(
    Guid ConfigurationId,
    Guid SystemId);