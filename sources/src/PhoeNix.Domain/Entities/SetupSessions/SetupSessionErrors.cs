using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.SetupSessions;

public static class SetupSessionErrors
{
    public static Error NotFound(SetupSessionId sessionId)
    {
        return new Error("SetupSessions.NotFound", $"Setup session '{sessionId}' was not found.");
    }

    public static Error NoSessionAvailable()
    {
        return new Error("SetSessions.NotAvailable", "No available setup session found");
    }
}