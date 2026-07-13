using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Infrastructure.Services.Virtualization;

public sealed class VirtualizationProviderFactory(
    IEnumerable<IVirtualizationProvider> providers) : IVirtualizationProviderFactory
{
    private readonly Dictionary<VmHostProvider, IVirtualizationProvider> _providers =
        providers.ToDictionary(p => p.ProviderType);

    public IVirtualizationProvider GetProvider(VmHostProvider providerType)
    {
        if (_providers.TryGetValue(providerType, out var provider))
            return provider;

        throw new InvalidOperationException(
            $"No virtualization provider registered for '{providerType}'.");
    }
}
