using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Commands;

public record RegisterVmHostCommand(
    string Name,
    VmHostProvider Provider,
    string Host,
    int? Port,
    string? Username,
    string? Secret,
    string? ExtraConfig) : ICommand<string>;

internal sealed class RegisterVmHostHandler(
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<RegisterVmHostCommand, string>
{
    public async Task<Result<string>> Handle(RegisterVmHostCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<string>(userIdResult.Error);

        var existingHost = await vmHostRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingHost is not null)
            return Result.Failure<string>(new Error(
                "VmHosts.NameAlreadyExists",
                $"VM host with name '{request.Name}' already exists."));

        var credential = VmHostCredential.Create(
            request.Host, request.Port, request.Username, request.Secret, request.ExtraConfig);

        var provider = providerFactory.GetProvider(request.Provider);
        var connectionResult = await provider.TestConnectionAsync(credential, cancellationToken);
        if (connectionResult.IsFailure)
            return Result.Failure<string>(connectionResult.Error);

        return VmHost
            .Create(
                new VmHostId(Guid.NewGuid()),
                userIdResult.Value,
                request.Name,
                request.Provider,
                credential)
            .Tap(vmHostRepository.Add)
            .Map(host => host.Id.Value.ToString());
    }
}
