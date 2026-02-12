using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface INixFormatterService
{
    public Result<string> FormatNixFilesInPlace(string path, CancellationToken cancellationToken);
}