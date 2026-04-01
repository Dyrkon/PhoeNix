namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IAuthenticationInvalidationNotifier
{
    event Action? AuthenticationInvalidated;

    void NotifyAuthenticationInvalidated();
}