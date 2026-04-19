using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationInputCommand(
    ConfigurationId ConfigurationId,
    InputId InputId,
    string Source,
    string Name,
    IReadOnlyList<InputFollowUpsertModel> Follows) : ICommand<InputResponse>;

internal sealed class UpdateConfigurationInputHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<UpdateConfigurationInputCommand, InputResponse>
{
    public async Task<Result<InputResponse>> Handle(
        UpdateConfigurationInputCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.UpdateInput(
                request.InputId,
                request.Source,
                request.Name,
                request.Follows.Select(InputMappings.MapInputFollowToDomain).ToList()))
            .Map(InputMappings.MapInputToDto);
    }
}