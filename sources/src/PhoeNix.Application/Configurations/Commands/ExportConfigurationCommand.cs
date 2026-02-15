using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record ExportConfigurationCommand(ConfigurationId ConfigurationId) : ICommand<string>;

internal sealed class ExportConfigurationCommandHandler(
    IConfigurationRepository configurationRepository,
    IConfigurationFilesBuilder configurationFilesBuilder,
    IFileSystemService fileSystemService,
    INixBuildMaterializer nixBuildMaterializer,
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<ExportConfigurationCommand, string>
{
    public async Task<Result<string>> Handle(ExportConfigurationCommand command, CancellationToken cancellationToken)
    {
        var moduleTemplates = await moduleTemplateRepository.GetAllAsync(cancellationToken)
            .EnsureNotNull(new Error("", "Cannot get module templates"));

        return await configurationRepository.GetByIdAsync(command.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("", $"Configuration {command.ConfigurationId} not found!"))
            .Bind(configuration =>
                nixBuildMaterializer.MaterializeConfiguration(configuration, moduleTemplates.Value.ToList()))
            .Bind(configurationFilesBuilder.BuildConfigurationFiles)
            .Bind(cFolder =>
                fileSystemService.WriteConfigurationToFs(cFolder, command.ConfigurationId, cancellationToken));
    }
}