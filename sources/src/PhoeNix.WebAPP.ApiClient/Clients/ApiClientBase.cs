using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    private readonly IAuthenticationInvalidationNotifier _authenticationInvalidationNotifier;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiClientBase(
        HttpClient httpClient,
        IAuthenticationInvalidationNotifier authenticationInvalidationNotifier)
    {
        HttpClient = httpClient;
        _authenticationInvalidationNotifier = authenticationInvalidationNotifier;
    }

    protected Task<ApiResult> PostAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(HttpMethod.Post, uri, null, cancellationToken);
    }

    protected Task<ApiResult> PostAsync<TRequest>(
        string uri,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(HttpMethod.Post, uri, body, cancellationToken);
    }

    protected Task<ApiResult> PutAsync<TRequest>(
        string uri,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(HttpMethod.Put, uri, body, cancellationToken);
    }

    protected Task<ApiResult> DeleteAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(HttpMethod.Delete, uri, null, cancellationToken);
    }

    protected async Task<ApiResult<TResponse>> PutWithResponseAsync<TResponse>(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, uri);
        request.Content = JsonContent.Create(body);

        using var response = await HttpClient.SendAsync(request, cancellationToken);

        NotifyIfAuthenticationInvalid(response);

        if (!response.IsSuccessStatusCode)
            return ApiResult<TResponse>.Failure(await ReadErrorAsync(response, cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return payload is null
            ? ApiResult<TResponse>.Failure(new ApiError(
                "ResponseBodyInvalid",
                "The server returned an invalid response body."))
            : ApiResult<TResponse>.Success(payload);
    }

    protected Task<ApiResult<TResponse>> GetAsync<TResponse>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Get, uri, null, cancellationToken);
    }

    protected Task<ApiResult<TResponse>> PostForValueAsync<TResponse>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Post, uri, null, cancellationToken);
    }

    protected async Task<ApiResult<TResponse>> PostWithResponseAsync<TResponse>(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, uri);
        request.Content = JsonContent.Create(body);

        using var response = await HttpClient.SendAsync(request, cancellationToken);

        NotifyIfAuthenticationInvalid(response);

        if (!response.IsSuccessStatusCode)
            return ApiResult<TResponse>.Failure(await ReadErrorAsync(response, cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return payload is null
            ? ApiResult<TResponse>.Failure(new ApiError(
                "ResponseBodyInvalid",
                "The server returned an invalid response body."))
            : ApiResult<TResponse>.Success(payload);
    }

    private async Task<ApiResult> SendAsync(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, uri);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await HttpClient.SendAsync(request, cancellationToken);

        NotifyIfAuthenticationInvalid(response);

        if (response.IsSuccessStatusCode)
            return ApiResult.Success();

        return ApiResult.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        HttpMethod method,
        string uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, uri);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await HttpClient.SendAsync(request, cancellationToken);

        NotifyIfAuthenticationInvalid(response);

        if (!response.IsSuccessStatusCode)
            return ApiResult<TResponse>.Failure(await ReadErrorAsync(response, cancellationToken));

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(payload))
            return ApiResult<TResponse>.Failure(new ApiError(
                "ResponseBodyMissing",
                "The server returned an empty response body."));

        var value = JsonSerializer.Deserialize<TResponse>(payload, JsonOptions);

        return value is null
            ? ApiResult<TResponse>.Failure(new ApiError(
                "ResponseBodyInvalid",
                "The server returned an invalid response body."))
            : ApiResult<TResponse>.Success(value);
    }

    private void NotifyIfAuthenticationInvalid(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            _authenticationInvalidationNotifier.NotifyAuthenticationInvalidated();
    }

    private const string ApiPrefix = "api/";

    protected static HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var cleanUri = uri.TrimStart('/');

        var finalUri = cleanUri.StartsWith(ApiPrefix)
            ? cleanUri
            : $"{ApiPrefix}{cleanUri}";

        var request = new HttpRequestMessage(method, finalUri);
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }

    protected static async Task<ApiError> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(payload))
            {
                var error = JsonSerializer.Deserialize<ApiError>(payload, JsonOptions);

                if (error is not null)
                    return error;
            }
        }
        catch
        {
        }

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ApiError("BadRequest", "The request was invalid."),
            HttpStatusCode.Unauthorized => new ApiError("Unauthorized", "You are not authorized."),
            HttpStatusCode.Forbidden => new ApiError("Forbidden", "Access was denied."),
            HttpStatusCode.NotFound => new ApiError("NotFound", "The requested resource was not found."),
            HttpStatusCode.Conflict => new ApiError("Conflict", "The request conflicts with current state."),
            _ => new ApiError(
                "HttpRequestFailed",
                $"The request failed with status code {(int)response.StatusCode}.")
        };
    }
}