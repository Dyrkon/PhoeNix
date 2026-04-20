using PhoeNix.Contracts.Auth;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IAuthenticationApiClient
{
    Task<ApiResult<AuthenticatedUserResponse>> LoginAsync(
        UserLoginRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<AuthenticatedUserResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<AuthenticatedUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResult> LogoutAsync(
        CancellationToken cancellationToken = default);
}
