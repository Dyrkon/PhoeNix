using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Queries;

public sealed record DiscoverVmsQuery(Guid VmHostId) : IQuery<IReadOnlyList<DiscoveredVmResponse>>;

internal sealed class DiscoverVmsQueryHandler(
    IVmHostRepository vmHostRepository,
    IMachineRepository machineRepository,
    IVirtualizationProviderFactory providerFactory)
    : IQueryHandler<DiscoverVmsQuery, IReadOnlyList<DiscoveredVmResponse>>
{
    public async Task<Result<IReadOnlyList<DiscoveredVmResponse>>> Handle(
        DiscoverVmsQuery request,
        CancellationToken cancellationToken)
    {
        var vmHostId = new VmHostId(request.VmHostId);
        var vmHost = await vmHostRepository.GetByIdAsync(vmHostId, cancellationToken);
        if (vmHost is null)
            return Result.Failure<IReadOnlyList<DiscoveredVmResponse>>(
                new Error("VmHosts.NotFound", "VM host not found."));

        var provider = providerFactory.GetProvider(vmHost.Provider);
        var listResult = await provider.ListVmsAsync(vmHost.Credential, cancellationToken);
        if (listResult.IsFailure)
            return Result.Failure<IReadOnlyList<DiscoveredVmResponse>>(listResult.Error);

        var linkedMachines = await machineRepository.GetAllByVmHostIdAsync(vmHostId, cancellationToken);
        var linkedByExternalId = linkedMachines
            .Where(m => m.ManagementProfile is not null)
            .ToDictionary(m => m.ManagementProfile!.ExternalId, m => (Guid?)m.Id.Value);

        var responses = listResult.Value.Select(vm => new DiscoveredVmResponse(
            vm.ExternalId,
            vm.Name,
            vm.CpuCores,
            vm.MemoryMb,
            vm.MacAddress,
            vm.PowerState,
            linkedByExternalId.GetValueOrDefault(vm.ExternalId))).ToList();

        return Result.Success<IReadOnlyList<DiscoveredVmResponse>>(responses);
    }
}
