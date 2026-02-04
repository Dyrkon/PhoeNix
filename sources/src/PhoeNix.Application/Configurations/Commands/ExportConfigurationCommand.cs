using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record ExportConfigurationCommand(ConfigurationId ConfigurationId) : ICommand<string>;

internal sealed class ExportConfigurationCommandHandler(
    IConfigurationRepository configurationRepository,
    IConfigurationBuilderService configurationBuilderService,
    IFileSystemService fileSystemService)
    : ICommandHandler<ExportConfigurationCommand, string>
{
    public async Task<Result<string>> Handle(ExportConfigurationCommand command, CancellationToken cancellationToken)
    {
        var config = await configurationRepository.GetByIdAsync(command.ConfigurationId, cancellationToken);

        if (config is null)
            return Result.Failure<string>(new Error("", $"Configuration {command.ConfigurationId} not found!"));

        return config.Build()
            .Bind(configurationBuilderService.BuildConfiguration)
            .Bind(cFolder => fileSystemService.WriteConfigurationToFs(cFolder, command.ConfigurationId));
    }
}