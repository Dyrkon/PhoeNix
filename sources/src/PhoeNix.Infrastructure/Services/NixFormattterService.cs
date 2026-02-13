using System.Diagnostics;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Services;

namespace PhoeNix.Infrastructure.Services;

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
            .RunProcess("nix", arguments, cancellationToken, timeOut: TimeSpan.FromMinutes(3), workingDirectory: path)
            .Map(_ => path);
    }
}