using PhoeNix.Domain.Models.Processes;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Processes;

public interface IProcessRunner
{
    public Result<ProcessResult> RunProcess(string executableName, List<string> arguments,
        CancellationToken cancellationToken, string? workingDirectory = null, string? standardInput = null,
        Action<string?>? perLineAction = null, TimeSpan? timeOut = null);
}