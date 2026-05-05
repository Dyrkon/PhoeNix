using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record RemoveConfigurationInputCommand(
    ConfigurationId ConfigurationId,
    InputId InputId) : ICommand;

internal sealed class RemoveConfigurationInputHandler(
    IConfigurationRepository configurationRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<RemoveConfigurationInputCommand>
{
    public async Task<Result> Handle(
        RemoveConfigurationInputCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Ensure(c => c.OwnerId == userIdResult.Value, ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.RemoveInput(request.InputId));
    }
}
