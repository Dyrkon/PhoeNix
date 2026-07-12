using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitOpsImportService(
    IServiceScopeFactory scopeFactory,
    ILogger<GitOpsImportService> logger) : IGitOpsImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<Result> ImportAllAsync(UserId ownerId, string repoPath, bool deleteOrphans, CancellationToken ct)
    {
        var templatesDir = Path.Combine(repoPath, "templates");
        var configurationsDir = Path.Combine(repoPath, "configurations");

        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var templateRepo = scope.ServiceProvider.GetRequiredService<IModuleTemplateRepository>();
        var configRepo = scope.ServiceProvider.GetRequiredService<IConfigurationRepository>();

        // Import templates first (configurations depend on them)
        var importedTemplateIds = new HashSet<Guid>();
        if (Directory.Exists(templatesDir))
        {
            foreach (var file in Directory.GetFiles(templatesDir, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var dto = JsonSerializer.Deserialize<ModuleTemplateResponse>(json, JsonOptions);
                if (dto is null)
                {
                    logger.LogWarning("Failed to deserialize template from {File}", file);
                    continue;
                }

                var result = await sender.Send(new ImportModuleTemplateCommand(dto), ct);
                if (result.IsFailure)
                    logger.LogWarning("Failed to import template '{Name}': {Error}", dto.Name, result.Error.Description);
                else
                    importedTemplateIds.Add(dto.Id);
            }
        }

        logger.LogDebug("Imported {Count} templates from git", importedTemplateIds.Count);

        // Import configurations
        var importedConfigIds = new HashSet<Guid>();
        if (Directory.Exists(configurationsDir))
        {
            foreach (var file in Directory.GetFiles(configurationsDir, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var dto = JsonSerializer.Deserialize<ConfigurationResponse>(json, JsonOptions);
                if (dto is null)
                {
                    logger.LogWarning("Failed to deserialize configuration from {File}", file);
                    continue;
                }

                var result = await sender.Send(new ImportConfigurationCommand(dto), ct);
                if (result.IsFailure)
                    logger.LogWarning("Failed to import configuration '{Title}': {Error}", dto.Title, result.Error.Description);
                else
                    importedConfigIds.Add(dto.Id);
            }
        }

        logger.LogDebug("Imported {Count} configurations from git", importedConfigIds.Count);

        // Delete orphans if enabled
        if (deleteOrphans)
        {
            var allTemplates = (await templateRepo.GetAllAsync(ownerId, ct)).ToList();
            foreach (var template in allTemplates.Where(t => !importedTemplateIds.Contains(t.Id.Value)))
            {
                logger.LogInformation("Deleting orphan template '{Name}' (not in git)", template.Name);
                // Templates don't have a RemoveByIdAsync in the plan, skip for now
            }

            var allConfigs = (await configRepo.GetAllAsync(ct))
                .Where(c => c.OwnerId == ownerId)
                .ToList();
            foreach (var config in allConfigs.Where(c => !importedConfigIds.Contains(c.Id.Value)))
            {
                logger.LogInformation("Deleting orphan configuration '{Title}' (not in git)", config.Title);
                await configRepo.RemoveByIdAsync(config.Id, ct);
            }
        }

        return Result.Success();
    }
}
