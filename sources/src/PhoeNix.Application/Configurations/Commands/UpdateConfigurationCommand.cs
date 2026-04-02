using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationCommand(
    ConfigurationId ConfigurationId,
    string Title,
    string Description) : ICommand<ConfigurationResponse>;

internal sealed class UpdateConfigurationHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<UpdateConfigurationCommand, ConfigurationResponse>
{
    public async Task<Result<ConfigurationResponse>> Handle(
        UpdateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ConfigurationResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        var result = configuration.EditConfiguration(request.Title, request.Description);

        if (result.IsFailure)
            return Result.Failure<ConfigurationResponse>(result.Error);

        return ConfigurationMappings.MapConfigurationToDto(configuration);
    }
}