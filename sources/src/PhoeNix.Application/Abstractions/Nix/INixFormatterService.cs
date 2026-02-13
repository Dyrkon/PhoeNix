using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixFormatterService
{
    public Result<string> FormatNixFilesInPlace(string path, CancellationToken cancellationToken);
}