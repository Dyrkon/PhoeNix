using FluentAssertions;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.Processes;
using PhoeNix.Application.Models.Tests;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure.Tests.Services;

public class NixTestRunnerTests
{
    [Fact]
    public void RunModuleTest_Should_Run_Nix_Build_And_Parse_Result_With_Trimmed_Output()
    {
        // Arrange
        var id = new TestId(Guid.NewGuid());
        var testName = "test01";
        var arch = Architecture.X86Linux;
        var path = "/tmp/phoenix";

        var processRunner = new FakeProcessRunner
        {
            NextResult = Result.Success(new ProcessResult(
                0,
                "  stdout \n",
                "  stderr \n",
                TimeSpan.FromMilliseconds(10),
                false,
                false,
                DateTime.UtcNow))
        };

        var parser = new FakeNixErrorParserService
        {
            NextResult = Result.Success(new ModuleTestResponse(
                id,
                testName,
                true,
                Array.Empty<ModuleTestErrorResponse>()))
        };

        var sut = new NixTestRunner(parser, processRunner);

        // Act
        var result = sut.RunModuleTest(id, testName, arch, path, CancellationToken.None);

        // Assert - returned response
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be(testName);
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();

        // Assert - process invocation
        processRunner.Calls.Should().ContainSingle();
        var call = processRunner.Calls.Single();

        call.ExecutableName.Should().Be("nix");
        call.Arguments.Should().Equal(
            "build",
            $"{path}#checks.{arch.ToArchitectureString()}.{id.ToStringWithPrefix()}",
            "-L",
            "--quiet");
        call.TimeOut.Should().Be(TimeSpan.FromMinutes(3));

        // Assert - parser invocation (trimmed)
        parser.Calls.Should().ContainSingle();
        var parse = parser.Calls.Single();

        parse.Id.Should().Be(id);
        parse.TestName.Should().Be(testName);
        parse.TestOutput.Should().Be("stdout");
        parse.ErrorOutput.Should().Be("stderr");
        parse.ExitCode.Should().Be(0);
    }

    [Fact]
    public void RunSystemTest_Should_Run_NixosAnywhere_And_Parse_BuildTime_From_Output()
    {
        // Arrange
        var systemId = new SystemId(Guid.NewGuid());
        var arch = Architecture.X86Linux;
        var path = "/tmp/phoenix";

        var processRunner = new FakeProcessRunner
        {
            NextResult = Result.Success(new ProcessResult(
                0,
                "",
                "",
                TimeSpan.FromSeconds(1),
                false,
                false,
                DateTime.UtcNow)),
            OnCall = call =>
            {
                // Simulate nixos-anywhere output lines
                call.PerLineAction?.Invoke("some log line");
                call.PerLineAction?.Invoke("test script finished in 12.3s");
                call.PerLineAction?.Invoke("other line");
            }
        };

        var parser = new FakeNixErrorParserService(); // unused by RunSystemTest
        var sut = new NixTestRunner(parser, processRunner);

        // Act
        var result = sut.RunSystemTest(systemId, arch, path, CancellationToken.None);

        // Assert - returned response
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(systemId);
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.BuildTime.Should().Be("12.3s");

        // Assert - process invocation
        processRunner.Calls.Should().ContainSingle();
        var call = processRunner.Calls.Single();

        call.ExecutableName.Should().Be("nixos-anywhere");
        call.Arguments.Should().Equal(
            "-L",
            "--",
            "--flake",
            $"{path}#{systemId.ToStringWithPrefix()}",
            "--vm-test");
        call.TimeOut.Should().Be(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public void RunSystemTest_Should_Return_Unspecified_When_No_Duration_Line_Present()
    {
        // Arrange
        var systemId = new SystemId(Guid.NewGuid());
        var arch = Architecture.X86Linux;
        var path = "/tmp/phoenix";

        var processRunner = new FakeProcessRunner
        {
            NextResult = Result.Success(new ProcessResult(
                0,
                "",
                "",
                TimeSpan.FromSeconds(1),
                false,
                false,
                DateTime.UtcNow)),
            OnCall = call =>
            {
                call.PerLineAction?.Invoke("no matching line here");
                call.PerLineAction?.Invoke("still nothing");
            }
        };

        var sut = new NixTestRunner(new FakeNixErrorParserService(), processRunner);

        // Act
        var result = sut.RunSystemTest(systemId, arch, path, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuildTime.Should().Be("unspecified");
    }

    // -------- fakes --------

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public Result<ProcessResult> NextResult { get; set; } =
            Result.Success(new ProcessResult(0, "", "", TimeSpan.Zero, false, false, DateTime.UtcNow));

        public List<Call> Calls { get; } = new();

        public Action<Call>? OnCall { get; set; }

        public Result<ProcessResult> RunProcess(
            string executableName,
            List<string> arguments,
            CancellationToken cancellationToken,
            string? workingDirectory = null,
            string? standardInput = null,
            Action<string?>? perLineAction = null,
            TimeSpan? timeOut = null)
        {
            var call = new Call(executableName, arguments, workingDirectory, standardInput, perLineAction, timeOut);
            Calls.Add(call);

            OnCall?.Invoke(call);

            return NextResult;
        }

        public record Call(
            string ExecutableName,
            List<string> Arguments,
            string? WorkingDirectory,
            string? StandardInput,
            Action<string?>? PerLineAction,
            TimeSpan? TimeOut);
    }

    private sealed class FakeNixErrorParserService : INixErrorParserService
    {
        public Result<ModuleTestResponse> NextResult { get; set; } =
            Result.Success(new ModuleTestResponse(new TestId(Guid.NewGuid()), "t", true,
                Array.Empty<ModuleTestErrorResponse>()));

        public List<ParseCall> Calls { get; } = new();

        public Result<ModuleTestResponse> ParseModelTestResult(
            TestId id,
            string testName,
            string testOutput,
            string errorOutput,
            int exitCode)
        {
            Calls.Add(new ParseCall(id, testName, testOutput, errorOutput, exitCode));
            return NextResult;
        }

        public record ParseCall(
            TestId Id,
            string TestName,
            string TestOutput,
            string ErrorOutput,
            int ExitCode);
    }
}