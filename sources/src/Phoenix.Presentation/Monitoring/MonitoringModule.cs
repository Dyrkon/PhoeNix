using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Monitoring.GetPrometheusTargets;

namespace Phoenix.Presentation.Monitoring;

public sealed class MonitoringModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/monitoring/targets", GetTargets)
            .AllowAnonymous()
            .Produces<IEnumerable<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetTargets(
        HttpContext context,
        IPrometheusTokenService tokenService,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (authHeader is null || !authHeader.StartsWith("Bearer ", StringComparison.Ordinal))
            return Results.Unauthorized();

        var token = authHeader["Bearer ".Length..];

        if (!await tokenService.ValidateTokenAsync(token))
            return Results.Unauthorized();

        var result = await sender.Send(new GetPrometheusTargetsQuery(), cancellationToken);

        if (result.IsFailure)
            return Results.Problem(result.Error.Description ?? result.Error.Code);

        var targets = result.Value.Select(t => new
        {
            targets = new[] { $"{t.Address}:{t.MetricsPort}" },
            labels = new Dictionary<string, string> { ["machine"] = t.Title }
        });

        return Results.Ok(targets);
    }
}
