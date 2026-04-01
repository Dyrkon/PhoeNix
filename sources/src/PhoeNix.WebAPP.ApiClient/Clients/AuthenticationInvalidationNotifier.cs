using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class AuthenticationInvalidationNotifier : IAuthenticationInvalidationNotifier
{
    public event Action? AuthenticationInvalidated;

    public void NotifyAuthenticationInvalidated()
    {
        AuthenticationInvalidated?.Invoke();
    }
}