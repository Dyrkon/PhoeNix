using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Commands;

public record TestVmHostConnectionCommand(Guid VmHostId) : ICommand;

internal sealed class TestVmHostConnectionHandler(
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory)
    : ICommandHandler<TestVmHostConnectionCommand>
{
    public async Task<Result> Handle(TestVmHostConnectionCommand request, CancellationToken cancellationToken)
    {
        var vmHost = await vmHostRepository.GetByIdAsync(new VmHostId(request.VmHostId), cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "VM host not found."));

        var provider = providerFactory.GetProvider(vmHost.Provider);
        return await provider.TestConnectionAsync(vmHost.Credential, cancellationToken);
    }
}
