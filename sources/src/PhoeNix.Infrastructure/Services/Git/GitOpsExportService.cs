using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Files;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Utilities;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitOpsExportService(
    IServiceScopeFactory scopeFactory,
    IGitOpsConfigurationFilesBuilder configurationFilesBuilder,
    INixBuildMaterializer nixBuildMaterializer,
    ILogger<GitOpsExportService> logger) : IGitOpsExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<Result> ExportAllAsync(UserId ownerId, string repoPath, bool includeNixFiles, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var configRepo = scope.ServiceProvider.GetRequiredService<IConfigurationRepository>();
        var templateRepo = scope.ServiceProvider.GetRequiredService<IModuleTemplateRepository>();

        var allTemplates = (await templateRepo.GetAllAsync(ownerId, ct)).ToList();
        var templatesById = allTemplates.ToDictionary(t => t.Id);
        var allConfigurations = (await configRepo.GetAllAsync(ct))
            .Where(c => c.OwnerId == ownerId)
            .ToList();

        // Clear and recreate export directories
        var templatesDir = Path.Combine(repoPath, "templates");
        var configurationsDir = Path.Combine(repoPath, "configurations");

        ClearDirectory(templatesDir);
        ClearDirectory(configurationsDir);

        // Export templates
        foreach (var template in allTemplates)
        {
            var dto = ModuleMappings.MapModuleToDto(template);
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            var fileName = $"{SlugGenerator.ToSlug(template.Name)}.json";
            await File.WriteAllTextAsync(Path.Combine(templatesDir, fileName), json, ct);
        }

        logger.LogDebug("Exported {Count} templates to {Path}", allTemplates.Count, templatesDir);

        // Export configurations
        foreach (var configuration in allConfigurations)
        {
            var dto = ConfigurationMappings.MapConfigurationToDto(configuration, templatesById);
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            var fileName = $"{SlugGenerator.ToSlug(configuration.Title)}.json";
            await File.WriteAllTextAsync(Path.Combine(configurationsDir, fileName), json, ct);
        }

        logger.LogDebug("Exported {Count} configurations to {Path}", allConfigurations.Count, configurationsDir);

        // Optionally export Nix flake files with friendly names
        if (includeNixFiles)
        {
            var flakesDir = Path.Combine(repoPath, "flakes");
            ClearDirectory(flakesDir);

            foreach (var configuration in allConfigurations)
            {
                var materializeResult = nixBuildMaterializer.MaterializeConfiguration(configuration, allTemplates);
                if (materializeResult.IsFailure)
                {
                    logger.LogWarning("Failed to materialize configuration '{Title}': {Error}",
                        configuration.Title, materializeResult.Error.Description);
                    continue;
                }

                var buildFilesResult = configurationFilesBuilder.BuildConfigurationFiles(materializeResult.Value);
                if (buildFilesResult.IsFailure)
                {
                    logger.LogWarning("Failed to build files for configuration '{Title}': {Error}",
                        configuration.Title, buildFilesResult.Error.Description);
                    continue;
                }

                var configFlakeDir = Path.Combine(flakesDir, buildFilesResult.Value.Name);
                Directory.CreateDirectory(configFlakeDir);
                await WriteFolderContentsAsync(configFlakeDir, buildFilesResult.Value, ct);
            }

            logger.LogDebug("Exported Nix flake files to {Path}", flakesDir);
        }

        return Result.Success();
    }

    private static void ClearDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }

    private static async Task WriteFolderContentsAsync(string rootPath, Folder folder, CancellationToken ct)
    {
        foreach (var file in folder.Files)
        {
            var path = Path.Combine(rootPath, file.Name);

            if (file.IsFolder)
            {
                Directory.CreateDirectory(path);
                await WriteFolderContentsAsync(path, (Folder)file, ct);
                continue;
            }

            await File.WriteAllTextAsync(path, ((TextFile)file).Content, ct);
        }
    }
}
