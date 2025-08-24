using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record RemoveConfigurationCommand(ConfigurationId Id) : ICommand;

internal sealed class RemoveConfigurationCommandHandler(IConfigurationRepository configurationRepository)
    : ICommandHandler<RemoveConfigurationCommand>
{
    public async Task<Result> Handle(RemoveConfigurationCommand request, CancellationToken cancellationToken)
    {
        var conf = await configurationRepository.GetByDescriptionAsync("string", cancellationToken);
        if (conf != null)
            Console.WriteLine(conf.Description);


        return await configurationRepository.RemoveByIdAsync(request.Id, cancellationToken);
    }
}