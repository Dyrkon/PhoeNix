using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
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
        var moduleTemplates = await moduleTemplateRepository.GetAllAsync(cancellationToken);

        if (moduleTemplates is null || !moduleTemplates.Any())
            return Result.Failure<string>(new Error("", "Cannot get module templates"));

        return await configurationRepository.GetByIdAsync(command.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("", $"Configuration {command.ConfigurationId} not found!"))
            .Bind(configuration =>
                nixBuildMaterializer.MaterializeConfiguration(configuration, moduleTemplates.ToList()))
            .Bind(configurationFilesBuilder.BuildConfigurationFiles)
            .Bind(cFolder =>
                fileSystemService.WriteConfigurationToFsAsync(cFolder, command.ConfigurationId, null, cancellationToken));
    }
}