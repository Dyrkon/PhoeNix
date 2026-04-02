using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record RemoveConfigurationCommand(ConfigurationId Id) : ICommand;

internal sealed class RemoveConfigurationCommandHandler(IConfigurationRepository configurationRepository)
    : ICommandHandler<RemoveConfigurationCommand>
{
    public async Task<Result> Handle(RemoveConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await configurationRepository.GetByIdAsync(request.Id, cancellationToken)
            .EnsureNotNull(new Error($"", $"Configuration {request.Id} not found!"))
            .Tap(conf => configurationRepository.RemoveByIdAsync(conf.Id, cancellationToken));
    }
}