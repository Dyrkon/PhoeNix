using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Behaviors;

public sealed class RequestLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogDebug("Handling {RequestName}", requestName);

        var response = await next();

        stopwatch.Stop();

        if (response is Result result && result.IsFailure)
        {
            logger.LogWarning(
                "Handled {RequestName} failed in {ElapsedMilliseconds}ms with {ErrorCode}: {ErrorDescription}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                result.Error.Code,
                result.Error.Description);

            return response;
        }

        logger.LogInformation(
            "Handled {RequestName} in {ElapsedMilliseconds}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}