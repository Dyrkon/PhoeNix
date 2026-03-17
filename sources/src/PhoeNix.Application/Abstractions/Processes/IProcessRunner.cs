using PhoeNix.Application.Models.Processes;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Processes;

public interface IProcessRunner
{
    public Result<ProcessResult> RunProcess(string executableName, List<string> arguments,
        CancellationToken cancellationToken, Dictionary<string, string>? environmentVariables = null,
        string? workingDirectory = null, string? standardInput = null,
        Action<string?>? perLineAction = null, TimeSpan? timeOut = null);
}