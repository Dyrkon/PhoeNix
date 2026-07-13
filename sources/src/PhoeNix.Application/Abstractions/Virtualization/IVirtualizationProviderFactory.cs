using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Abstractions.Virtualization;

public interface IVirtualizationProviderFactory
{
    IVirtualizationProvider GetProvider(VmHostProvider providerType);
}
