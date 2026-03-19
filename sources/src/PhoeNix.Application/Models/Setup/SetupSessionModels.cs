using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Application.Models.Setup;

public sealed record StartMachineSetupRequest(
    Guid ConfigurationId,
    Guid SystemId);

public sealed record CallbackModuleParameters(
    string FinalizeUrl,
    string BearerToken);

public sealed record BuiltInModuleParameters(
    CallbackModuleParameters? Callback = null);