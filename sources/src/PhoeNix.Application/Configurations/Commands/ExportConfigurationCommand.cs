using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
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
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService)
    : ICommandHandler<ExportConfigurationCommand, string>
{
    public async Task<Result<string>> Handle(ExportConfigurationCommand command, CancellationToken cancellationToken)
    {
        return await configurationRepository.GetByIdAsync(command.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("", $"Configuration {command.ConfigurationId} not found!"))
            .Bind(config => config.Build())
            .Bind(configurationFilesBuilder.BuildConfiguration)
            .Bind(cFolder =>
                fileSystemService.WriteConfigurationToFs(cFolder, command.ConfigurationId, cancellationToken));
    }
}