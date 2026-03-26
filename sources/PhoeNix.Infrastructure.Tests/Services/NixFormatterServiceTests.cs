using FluentAssertions;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.Processes;
using PhoeNix.Domain.Shared;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure.Tests.Services;

public class NixFormatterServiceTests
{
    [Fact]
    public void FormatNixFilesInPlace_Should_Fail_When_Directory_Does_Not_Exist()
    {
        // Arrange
        var processRunner = new FakeProcessRunner();
        var sut = new NixFormatterService(processRunner);

        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        // Act
        var result = sut.FormatNixFilesInPlace(path, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InvalidPath");
        result.Error.Description.Should().Contain(path);
        processRunner.Calls.Should().BeEmpty();
    }

    [Fact]
    public void FormatNixFilesInPlace_Should_Run_Nix_Fmt_And_Return_Path()
    {
        // Arrange
        var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            var processRunner = new FakeProcessRunner
            {
                NextResult = Result.Success(new ProcessResult(
                    0,
                    "ok",
                    "",
                    TimeSpan.FromMilliseconds(10),
                    false,
                    false,
                    DateTime.UtcNow))
            };

            var sut = new NixFormatterService(processRunner);

            // Act
            var result = sut.FormatNixFilesInPlace(tempDir.FullName, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(tempDir.FullName);

            processRunner.Calls.Should().ContainSingle();
            var call = processRunner.Calls.Single();

            call.ExecutableName.Should().Be("nix");
            call.Arguments.Should().Equal("fmt", tempDir.FullName);
            call.WorkingDirectory.Should().Be(tempDir.FullName);
            call.TimeOut.Should().Be(TimeSpan.FromMinutes(3));
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public Result<ProcessResult> NextResult { get; set; } =
            Result.Success(new ProcessResult(0, "", "", TimeSpan.Zero, false, false, DateTime.UtcNow));

        public List<Call> Calls { get; } = new();

        public Result<ProcessResult> RunProcess(
            string executableName,
            List<string> arguments,
            CancellationToken cancellationToken,
            Dictionary<string, string>? environmentVariables = null,
            string? workingDirectory = null,
            string? standardInput = null,
            Action<string?>? perLineAction = null,
            TimeSpan? timeOut = null)
        {
            Calls.Add(new Call(executableName, arguments, workingDirectory, standardInput, timeOut));

            return NextResult;
        }

        public record Call(
            string ExecutableName,
            List<string> Arguments,
            string? WorkingDirectory,
            string? StandardInput,
            TimeSpan? TimeOut);
    }
}