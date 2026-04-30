using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.UtilityWrappers;

public class NixFormatterService(IProcessRunner processRunner) : INixFormatterService
{
    public Result<string> FormatNixFilesInPlace(string path, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path) && !File.Exists(path))
            return Result.Failure<string>(new Error("InvalidPath", $"Path '{path}' does not exist."));

        List<string> arguments = [path];

        var formatterPath = Environment.GetEnvironmentVariable("PHOENIX_ALEJANDRA_PATH") ?? "alejandra";

        return processRunner
            .RunProcess(formatterPath, arguments, cancellationToken, workingDirectory: path,
                timeOut: TimeSpan.FromMinutes(3))
            .Map(_ => path);
    }
}