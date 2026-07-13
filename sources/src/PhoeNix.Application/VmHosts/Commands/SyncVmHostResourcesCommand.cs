using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Commands;

public record SyncVmHostResourcesCommand(Guid VmHostId) : ICommand;

internal sealed class SyncVmHostResourcesHandler(
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory)
    : ICommandHandler<SyncVmHostResourcesCommand>
{
    public async Task<Result> Handle(SyncVmHostResourcesCommand request, CancellationToken cancellationToken)
    {
        var vmHost = await vmHostRepository.GetByIdAsync(new VmHostId(request.VmHostId), cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "VM host not found."));

        var provider = providerFactory.GetProvider(vmHost.Provider);
        var resourcesResult = await provider.GetResourcesAsync(vmHost.Credential, cancellationToken);
        if (resourcesResult.IsFailure)
            return resourcesResult.Error;

        return vmHost.UpdateResources(resourcesResult.Value, DateTime.UtcNow);
    }
}
