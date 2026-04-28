using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.UtilityWrappers;

public class NixFormatterService(IProcessRunner processRunner) : INixFormatterService
{
    public Result<string> FormatNixFilesInPlace(string path, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path))
            return Result.Failure<string>(new Error("InvalidPath", $"Directory '{path}' does not exist."));

        List<string> arguments =
        [
            "fmt",
            $"{path}"
        ];

        return processRunner
            .RunProcess("nix", arguments, cancellationToken, timeOut: TimeSpan.FromMinutes(3))
            .Map(_ => path);
    }
}