using System.Net.Http.Json;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;
using ApiError = PhoeNix.WebAPP.ApiClient.Contracts.ApiError;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class AuthenticationApiClient(HttpClient httpClient)
    : ApiClientBase(httpClient), IAuthenticationApiClient
{
    public Task<ApiResult<AuthenticatedUserResponse>> LoginAsync(
        UserLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostForValueAsync<AuthenticatedUserResponse>("auth/login", request, cancellationToken);
    }

    public Task<ApiResult<AuthenticatedUserResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostForValueAsync<AuthenticatedUserResponse>("auth/register", request, cancellationToken);
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

    private Task<ApiResult<TResponse>> PostForValueAsync<TResponse>(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        return SendTypedPostAsync<TResponse>(uri, body, cancellationToken);
    }

    private async Task<ApiResult<TResponse>> SendTypedPostAsync<TResponse>(
        string uri,
        object body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, uri);
        request.Content = JsonContent.Create(body);

        using var response = await HttpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return ApiResult<TResponse>.Failure(await ReadErrorAsync(response, cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return payload is null
            ? ApiResult<TResponse>.Failure(new ApiError(
                "ResponseBodyInvalid",
                "The server returned an invalid response body."))
            : ApiResult<TResponse>.Success(payload);
    }
}