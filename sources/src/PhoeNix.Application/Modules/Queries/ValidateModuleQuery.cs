using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Tests;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public record ValidateModuleQuery(
    ConfigurationId ConfigurationId,
    ModuleTemplateId ModuleTemplateId,
    Architecture Architecture)
    : IQuery<List<ModuleTestResponse>>;

internal sealed class ValidateModuleQueryHandler(
    INixTestRunner nixTestRunner,
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    IFileSystemService fileSystemService,
    ICurrentUserAccessor currentUserAccessor) : IQueryHandler<ValidateModuleQuery, List<ModuleTestResponse>>
{
    public async Task<Result<List<ModuleTestResponse>>> Handle(ValidateModuleQuery query, CancellationToken ct)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(userIdResult.Error);

        var configResult = await configurationRepository
            .GetByIdAsync(query.ConfigurationId, ct)
            .EnsureNotNull(new Error("ConfigurationNotFound", $"Configuration {query.ConfigurationId} not found!"))
            .Ensure(c => c.OwnerId == userIdResult.Value, new Error("ConfigurationNotFound", $"Configuration {query.ConfigurationId} not found!"));

        if (configResult.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(configResult.Error);

        var moduleResult = await moduleTemplateRepository
            .GetByIdAsync(query.ModuleTemplateId, ct)
            .EnsureNotNull(new Error("ModuleNotFound", $"Module {query.ModuleTemplateId} not found!"));

        if (moduleResult.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(moduleResult.Error);

        var config = configResult.Value;
        var module = moduleResult.Value;

        if (config.Modules.All(m => m.ModuleTemplateId != module.Id))
            return Result.Failure<List<ModuleTestResponse>>(new Error(
                "ModuleNotInConfiguration",
                $"Module {module.Name} is not in configuration {config.Title}"));

        if (!module.Tests.Any())
            return Result.Failure<List<ModuleTestResponse>>(new Error(
                "NoTestsInModule",
                $"Module {module.Name} does not have any tests."));

        var root = fileSystemService.GetRootFolder();
        if (root.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(root.Error);

        var configPath = $"{root.Value}/{config.Id.Value}";
        if (!Directory.Exists(configPath))
            return Result.Failure<List<ModuleTestResponse>>(new Error(
                "ConfigurationNotBuilt",
                $"Configuration {config.Title} is not built yet!"));

        var responses = new List<ModuleTestResponse>();

        foreach (var test in module.Tests)
        {
            var r = nixTestRunner.RunModuleTest(
                test.Id,
                test.Name,
                query.Architecture,
                configPath, ct);

            if (r.IsFailure)
                return Result.Failure<List<ModuleTestResponse>>(r.Error);

            responses.Add(r.Value);
        }

        return Result.Success(responses);
    }
}