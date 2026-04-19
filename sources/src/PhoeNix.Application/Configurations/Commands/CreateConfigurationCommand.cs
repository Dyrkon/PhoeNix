using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record CreateConfigurationCommand(
    string Title,
    string Description) : ICommand<ConfigurationResponse>;

internal sealed class CreateConfigurationHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<CreateConfigurationCommand, ConfigurationResponse>
{
    public Task<Result<ConfigurationResponse>> Handle(
        CreateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var configurationId = new ConfigurationId(Guid.NewGuid());


        var result = Configuration.Create(configurationId, request.Title, request.Description)
            .Tap(configurationRepository.Add)
            .Map(configuration =>
                ConfigurationMappings.MapConfigurationToDto(configuration,
                    new Dictionary<ModuleTemplateId, ModuleTemplate>()));

        return Task.FromResult(result);
    }
}