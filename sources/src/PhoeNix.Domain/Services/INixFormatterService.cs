using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface INixFormatterService
{
    public Result FormatNixInPlace(string path);
}