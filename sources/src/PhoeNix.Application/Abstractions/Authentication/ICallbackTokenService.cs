using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ICallbackTokenService
{
    Result<CallbackToken> Create(SetupSessionId sessionId, MachineId machineId, DateTime nowUtc, TimeSpan ttl);
    Task<Result<CallbackTokenContext>> ValidateAndDecode(string token, DateTime nowUtc);
}