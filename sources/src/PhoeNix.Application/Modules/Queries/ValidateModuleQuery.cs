using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public record ValidateModuleQuery(ConfigurationId ConfigurationId, ModuleId ModuleId, Architecture Architecture)
    : IQuery<List<ModuleTestResponse>>;

internal sealed class ValidateModuleQueryHandler(
    INixTestRunner nixTestRunner,
    IConfigurationRepository configurationRepository,
    IModuleRepository moduleRepository,
    IFileSystemService fileSystemService) : IQueryHandler<ValidateModuleQuery, List<ModuleTestResponse>>
{
    public async Task<Result<List<ModuleTestResponse>>> Handle(ValidateModuleQuery query, CancellationToken ct)
    {
        var configResult = await configurationRepository
            .GetByIdAsync(query.ConfigurationId, ct)
            .EnsureNotNull(new Error("ConfigurationNotFound", $"Configuration {query.ConfigurationId} not found!"));

        if (configResult.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(configResult.Error);

        var moduleResult = await moduleRepository
            .GetByIdAsync(query.ModuleId, ct)
            .EnsureNotNull(new Error("ModuleNotFound", $"Module {query.ModuleId} not found!"));

        if (moduleResult.IsFailure)
            return Result.Failure<List<ModuleTestResponse>>(moduleResult.Error);

        var config = configResult.Value;
        var module = moduleResult.Value;

        if (config.Modules.All(m => m.ModuleId != module.Id))
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
                test.TestId,
                test.Test.Name,
                query.Architecture,
                configPath, ct);

            if (r.IsFailure)
                return Result.Failure<List<ModuleTestResponse>>(r.Error);

            responses.Add(r.Value);
        }

        return Result.Success(responses);
    }
}