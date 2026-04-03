using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record AddConfigurationSystemCommand(
    ConfigurationId ConfigurationId,
    string Name,
    Architecture Architecture) : ICommand<SystemResponse>;

internal sealed class AddConfigurationSystemHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<AddConfigurationSystemCommand, SystemResponse>
{
    public async Task<Result<SystemResponse>> Handle(
        AddConfigurationSystemCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        var systemId = new SystemId(Guid.NewGuid());

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.AddSystem(systemId, request.Architecture, request.Name))
            .Map(SystemMappings.MapSystemToDto);
    }
}