using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PhoeNix.Application.Behaviors;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Behaviors;

public class RequestLoggingBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Log_And_Return_Success()
    {
        var logger = new NullLogger<RequestLoggingBehavior<TestRequest, Result>>();
        var behavior = new RequestLoggingBehavior<TestRequest, Result>(logger);
        var response = Result.Success();

        var result = await behavior.Handle(
            new TestRequest(),
            ct => Task.FromResult(response),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_Result()
    {
        var logger = new NullLogger<RequestLoggingBehavior<TestRequest, Result>>();
        var behavior = new RequestLoggingBehavior<TestRequest, Result>(logger);
        var response = Result.Failure(new Error("Test.Error", "Something failed"));

        var result = await behavior.Handle(
            new TestRequest(),
            ct => Task.FromResult(response),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Test.Error");
    }

    [Fact]
    public async Task Handle_Should_Pass_Through_Non_Result_Response()
    {
        var logger = new NullLogger<RequestLoggingBehavior<TestRequest, string>>();
        var behavior = new RequestLoggingBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(
            new TestRequest(),
            ct => Task.FromResult("response"),
            CancellationToken.None);

        result.Should().Be("response");
    }

    private sealed record TestRequest;
}
