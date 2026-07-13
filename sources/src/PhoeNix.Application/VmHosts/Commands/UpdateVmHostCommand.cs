using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Commands;

public record UpdateVmHostCommand(
    Guid VmHostId,
    string Name,
    string Host,
    int? Port,
    string? Username,
    string? Secret,
    string? ExtraConfig) : ICommand;

internal sealed class UpdateVmHostHandler(
    IVmHostRepository vmHostRepository)
    : ICommandHandler<UpdateVmHostCommand>
{
    public async Task<Result> Handle(UpdateVmHostCommand request, CancellationToken cancellationToken)
    {
        var vmHost = await vmHostRepository.GetByIdAsync(new VmHostId(request.VmHostId), cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "VM host not found."));

        var existingByName = await vmHostRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingByName is not null && existingByName.Id != vmHost.Id)
            return Result.Failure(new Error(
                "VmHosts.NameAlreadyExists",
                $"VM host with name '{request.Name}' already exists."));

        var nameResult = vmHost.ChangeName(request.Name);
        if (nameResult.IsFailure)
            return nameResult;

        var credential = VmHostCredential.Create(
            request.Host, request.Port, request.Username, request.Secret, request.ExtraConfig);

        return vmHost.UpdateCredential(credential);
    }
}
