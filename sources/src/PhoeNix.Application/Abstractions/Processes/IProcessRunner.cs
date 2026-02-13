using System.Diagnostics;
using PhoeNix.Domain.Models.Processes;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IProcessRunner
{
    public Result<ProcessResult> RunProcess(string executableName, List<string> arguments,
        CancellationToken cancellationToken, string? workingDirectory = null, string? standardInput = null,
        DataReceivedEventHandler? perLineAction = null, TimeSpan? timeOut = null);
}