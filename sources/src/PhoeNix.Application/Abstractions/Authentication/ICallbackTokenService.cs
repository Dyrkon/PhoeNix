using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ICallbackTokenService
{
    Result<CallbackToken> Create(ProvisioningSessionId sessionId, MachineId machineId, DateTime nowUtc, TimeSpan ttl);
    Task<Result<CallbackTokenContext>> ValidateAndDecode(string token, DateTime nowUtc);
}