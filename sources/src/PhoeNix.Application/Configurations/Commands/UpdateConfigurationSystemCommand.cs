using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationSystemCommand(
    ConfigurationId ConfigurationId,
    SystemId SystemId,
    string Name) : ICommand<SystemResponse>;

internal sealed class UpdateConfigurationSystemHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<UpdateConfigurationSystemCommand, SystemResponse>
{
    public async Task<Result<SystemResponse>> Handle(
        UpdateConfigurationSystemCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.UpdateSystem(request.SystemId, request.Name))
            .Map(SystemMappings.MapSystemToDto);
    }
}