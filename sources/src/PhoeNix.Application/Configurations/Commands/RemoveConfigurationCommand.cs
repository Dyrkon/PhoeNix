using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record RemoveConfigurationCommand(ConfigurationId Id) : ICommand;

internal sealed class RemoveConfigurationCommandHandler(
    IConfigurationRepository configurationRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<RemoveConfigurationCommand>
{
    public async Task<Result> Handle(RemoveConfigurationCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        return await configurationRepository.GetByIdAsync(request.Id, cancellationToken)
            .EnsureNotNull(ConfigurationErrors.NotFound(request.Id))
            .Ensure(c => c.OwnerId == userIdResult.Value, ConfigurationErrors.NotFound(request.Id))
            .Tap(conf => configurationRepository.RemoveByIdAsync(conf.Id, cancellationToken));
    }
}