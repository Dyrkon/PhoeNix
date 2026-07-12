using PhoeNix.Application.Abstractions.Authentication;
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
    IConfigurationRepository configurationRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<CreateConfigurationCommand, ConfigurationResponse>
{
    public async Task<Result<ConfigurationResponse>> Handle(
        CreateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ConfigurationResponse>(userIdResult.Error);

        var existing = await configurationRepository.GetByTitleAsync(request.Title, userIdResult.Value, cancellationToken);
        if (existing is not null)
            return Result.Failure<ConfigurationResponse>(ConfigurationErrors.TitleAlreadyExists(request.Title));

        var configurationId = new ConfigurationId(Guid.NewGuid());

        return Configuration.Create(configurationId, userIdResult.Value, request.Title, request.Description)
            .Tap(configurationRepository.Add)
            .Map(configuration =>
                ConfigurationMappings.MapConfigurationToDto(configuration,
                    new Dictionary<ModuleTemplateId, ModuleTemplate>()));
    }
}