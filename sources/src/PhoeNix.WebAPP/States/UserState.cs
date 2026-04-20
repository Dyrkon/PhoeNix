using PhoeNix.Contracts.Auth;

namespace PhoeNix.WebAPP.States;

public sealed class UserState
{
    public AuthenticatedUserResponse? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public bool IsInitialized { get; private set; }

    public event Action? Changed;

    public void SetCurrentUser(AuthenticatedUserResponse user)
    {
        CurrentUser = user;
        IsInitialized = true;
        NotifyStateChanged();
    }

    public void Clear()
    {
        CurrentUser = null;
        IsInitialized = true;
        NotifyStateChanged();
    }

    public void MarkInitialized()
    {
        IsInitialized = true;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        Changed?.Invoke();
    }
}