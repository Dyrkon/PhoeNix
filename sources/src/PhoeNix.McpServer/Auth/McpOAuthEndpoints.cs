using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.McpServer.Auth;

internal static class McpOAuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/.well-known/oauth-protected-resource", (HttpRequest request) =>
            Results.Ok(new
            {
                resource = BaseUrl(request),
                authorization_servers = new[] { BaseUrl(request) }
            }))
            .AllowAnonymous();

        app.MapGet("/.well-known/oauth-authorization-server", (HttpRequest request) =>
            Results.Ok(new
            {
                issuer = BaseUrl(request),
                authorization_endpoint = $"{BaseUrl(request)}/oauth/authorize",
                token_endpoint = $"{BaseUrl(request)}/oauth/token",
                registration_endpoint = $"{BaseUrl(request)}/oauth/register",
                response_types_supported = new[] { "code" },
                grant_types_supported = new[] { "authorization_code" },
                code_challenge_methods_supported = new[] { "S256" },
                token_endpoint_auth_methods_supported = new[] { "none" }
            }))
            .AllowAnonymous();

        app.MapPost("/oauth/register", async (HttpContext context) =>
        {
            using var doc = await JsonDocument.ParseAsync(context.Request.Body);
            var redirectUris = doc.RootElement.TryGetProperty("redirect_uris", out var urisEl)
                ? urisEl.EnumerateArray().Select(u => u.GetString() ?? string.Empty).ToArray()
                : [];

            return Results.Ok(new
            {
                client_id = Guid.NewGuid().ToString("N"),
                client_id_issued_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                redirect_uris = redirectUris,
                grant_types = new[] { "authorization_code" },
                response_types = new[] { "code" },
                token_endpoint_auth_method = "none"
            });
        }).AllowAnonymous();

        app.MapGet("/oauth/authorize", (
            string? response_type,
            string? redirect_uri,
            string? code_challenge,
            string? code_challenge_method,
            string? state,
            string? client_id) =>
        {
            if (response_type != "code"
                || string.IsNullOrEmpty(redirect_uri)
                || string.IsNullOrEmpty(code_challenge)
                || code_challenge_method != "S256")
                return Results.BadRequest("Invalid OAuth authorization request.");

            return Results.Content(
                LoginForm(redirect_uri, state ?? string.Empty, code_challenge, client_id ?? string.Empty, error: null),
                "text/html");
        }).AllowAnonymous();

        app.MapPost("/oauth/authorize", async (
            HttpContext context,
            McpAuthCodeStore codeStore,
            IUserRepository userRepository,
            IUserPasswordHasher passwordHasher) =>
        {
            var form = await context.Request.ReadFormAsync();
            var username = form["username"].FirstOrDefault() ?? string.Empty;
            var password = form["password"].FirstOrDefault() ?? string.Empty;
            var redirectUri = form["redirect_uri"].FirstOrDefault() ?? string.Empty;
            var state = form["state"].FirstOrDefault() ?? string.Empty;
            var codeChallenge = form["code_challenge"].FirstOrDefault() ?? string.Empty;
            var clientId = form["client_id"].FirstOrDefault() ?? string.Empty;

            var user = await userRepository.GetByNormalizedNameAsync(
                User.NormalizeName(username), context.RequestAborted);

            if (user is null || !passwordHasher.VerifyPassword(user, password))
            {
                return Results.Content(
                    LoginForm(redirectUri, state, codeChallenge, clientId, error: "Invalid username or password."),
                    "text/html");
            }

            var code = Guid.NewGuid().ToString("N");
            codeStore.Store(code, user.Id.Value, codeChallenge);

            var location = $"{redirectUri}?code={Uri.EscapeDataString(code)}&state={Uri.EscapeDataString(state)}";
            return Results.Redirect(location);
        }).AllowAnonymous();

        app.MapPost("/oauth/token", async (HttpContext context, McpAuthCodeStore codeStore, McpJwtService jwtService) =>
        {
            var form = await context.Request.ReadFormAsync();
            var grantType = form["grant_type"].FirstOrDefault();
            var code = form["code"].FirstOrDefault();
            var codeVerifier = form["code_verifier"].FirstOrDefault();

            if (grantType != "authorization_code"
                || string.IsNullOrEmpty(code)
                || string.IsNullOrEmpty(codeVerifier))
                return Results.BadRequest(new { error = "invalid_request" });

            var authCode = codeStore.Consume(code);
            if (authCode is null)
                return Results.BadRequest(new { error = "invalid_grant" });

            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            var computedChallenge = Convert.ToBase64String(hash)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            if (!string.Equals(computedChallenge, authCode.CodeChallenge, StringComparison.Ordinal))
                return Results.BadRequest(new { error = "invalid_grant" });

            var (token, expiresIn) = jwtService.IssueToken(authCode.UserId);
            return Results.Ok(new { access_token = token, token_type = "Bearer", expires_in = expiresIn });
        }).AllowAnonymous();
    }

    private static string BaseUrl(HttpRequest request) => $"{request.Scheme}://{request.Host}";

    private static string LoginForm(
        string redirectUri, string state, string codeChallenge, string clientId, string? error) =>
        $$"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <title>PhoeNix — Sign In</title>
          <style>
            * { box-sizing: border-box; }
            body { font-family: system-ui, sans-serif; display: flex; justify-content: center;
                   align-items: center; min-height: 100vh; margin: 0; background: #f5f5f5; }
            .card { background: white; padding: 2rem; border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0,0,0,.12); width: 340px; }
            h1 { margin: 0 0 1.5rem; font-size: 1.2rem; color: #333; }
            label { display: block; margin-bottom: .25rem; font-size: .85rem; color: #555; }
            input[type=text], input[type=password] { width: 100%; padding: .5rem .75rem;
                border: 1px solid #ccc; border-radius: 4px; margin-bottom: 1rem; font-size: 1rem; }
            input:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px #1976d220; }
            button { width: 100%; padding: .7rem; background: #1976d2; color: white;
                     border: none; border-radius: 4px; font-size: 1rem; cursor: pointer; }
            button:hover { background: #1565c0; }
            .error { color: #d32f2f; font-size: .875rem; margin-bottom: 1rem;
                     padding: .5rem .75rem; background: #fdecea; border-radius: 4px; }
          </style>
        </head>
        <body>
          <div class="card">
            <h1>Sign in to PhoeNix</h1>
            {{(error is null ? string.Empty : $"<div class=\"error\">{WebUtility.HtmlEncode(error)}</div>")}}
            <form method="post" action="/oauth/authorize">
              <input type="hidden" name="redirect_uri" value="{{WebUtility.HtmlEncode(redirectUri)}}">
              <input type="hidden" name="state" value="{{WebUtility.HtmlEncode(state)}}">
              <input type="hidden" name="code_challenge" value="{{WebUtility.HtmlEncode(codeChallenge)}}">
              <input type="hidden" name="client_id" value="{{WebUtility.HtmlEncode(clientId)}}">
              <label for="username">Username</label>
              <input type="text" id="username" name="username" autocomplete="username" autofocus>
              <label for="password">Password</label>
              <input type="password" id="password" name="password" autocomplete="current-password">
              <button type="submit">Sign In</button>
            </form>
          </div>
        </body>
        </html>
        """;
}
