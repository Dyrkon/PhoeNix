using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record AddConfigurationInputCommand(
    ConfigurationId ConfigurationId,
    string Source,
    string Name,
    IReadOnlyList<InputFollowUpsertModel> Follows) : ICommand<InputResponse>;

internal sealed class AddConfigurationInputHandler(
    IConfigurationRepository configurationRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<AddConfigurationInputCommand, InputResponse>
{
    public async Task<Result<InputResponse>> Handle(
        AddConfigurationInputCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<InputResponse>(userIdResult.Error);

        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Ensure(c => c.OwnerId == userIdResult.Value, ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(cfg => cfg.AddInput(request.Source, request.Name)
                .Bind(input => input.ReplaceFollows(
                        request.Follows.Select(InputMappings.MapInputFollowToDomain).ToList())
                    .Map(() => input)))
            .Map(InputMappings.MapInputToDto);
    }
}