using PhoeNix.Contracts.Auth;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class AuthenticationApiClient(
    HttpClient httpClient,
    IAuthenticationInvalidationNotifier authenticationInvalidationNotifier)
    : ApiClientBase(httpClient, authenticationInvalidationNotifier), IAuthenticationApiClient
{
    public Task<ApiResult<AuthenticatedUserResponse>> LoginAsync(
        UserLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostWithResponseAsync<AuthenticatedUserResponse>("auth/login", request, cancellationToken);
    }

    public Task<ApiResult<AuthenticatedUserResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostWithResponseAsync<AuthenticatedUserResponse>("auth/register", request, cancellationToken);
    }

    public Task<ApiResult<AuthenticatedUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<AuthenticatedUserResponse>("auth/me", cancellationToken);
    }

    public Task<ApiResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        return PostAsync("auth/logout", cancellationToken);
    }
}
