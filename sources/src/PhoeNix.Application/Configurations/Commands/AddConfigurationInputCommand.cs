using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Inputs;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record AddConfigurationInputCommand(
    ConfigurationId ConfigurationId,
    string Source,
    string Name,
    IReadOnlyList<InputFollowUpsertModel> Follows) : ICommand<InputResponse>;

internal sealed class AddConfigurationInputHandler(
    IConfigurationRepository configurationRepository)
    : ICommandHandler<AddConfigurationInputCommand, InputResponse>
{
    public async Task<Result<InputResponse>> Handle(
        AddConfigurationInputCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.AddInput(request.Source, request.Name)
                .Bind(input => input.ReplaceFollows(
                        request.Follows.Select(InputMappings.MapInputFollowToDomain).ToList())
                    .Map(() => input)))
            .Map(InputMappings.MapInputToDto);
    }
}