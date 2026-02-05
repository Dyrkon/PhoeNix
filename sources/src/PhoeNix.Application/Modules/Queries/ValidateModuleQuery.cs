using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Queries;

public record ValidateModuleQuery(ConfigurationId ConfigurationId, ModuleId ModuleId, Architecture Architecture)
    : IQuery;

internal sealed class ValidateModuleQueryHandler(
    IConfigurationTestRunnerService configurationTestRunnerService,
    IConfigurationRepository configurationRepository,
    IModuleRepository moduleRepository,
    IFileSystemService fileSystemService) : IQueryHandler<ValidateModuleQuery>
{
    public async Task<Result> Handle(ValidateModuleQuery query, CancellationToken cancellationToken)
    {
        return await configurationRepository.GetByIdAsync(query.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("ConfigurationNotFound", $"Configuration {query.ConfigurationId} not found!"))
            .Bind(config =>
                moduleRepository
                    .GetByIdAsync(query.ModuleId, cancellationToken)
                    .EnsureNotNull(new Error("ModuleNotFound", $"Module {query.ModuleId} not found!"))
                    .Bind(module => Result.Success((config, module))))
            .Ensure(
                x => x.config.Modules.Any(m => m.ModuleId == x.module.Id),
                x => new Error("ModuleNotInConfiguration",
                    $"Module {x.module.Name} is not in configuration {x.config.Title}"))
            .Ensure(
                x => x.module.Tests.Any(),
                x => new Error("NoTestsInModule", $"Module {x.module.Name} does not have any tests."))
            .Bind(x =>
                fileSystemService.GetRootFolder()
                    .Bind(root =>
                    {
                        var configPath = $"{root}/{x.config.Id.Value}";

                        if (!Directory.Exists(configPath))
                            return Result.Failure(new Error("ConfigurationNotBuilt",
                                $"Configuration {x.config.Title} is not built yet!"));

                        foreach (var test in x.module.Tests)
                        {
                            var r = configurationTestRunnerService.RunModuleTest(
                                test.TestId.ToStringWithPrefix(),
                                query.Architecture,
                                configPath);

                            if (r.IsFailure)
                                return r;
                        }

                        return Result.Success();
                    }));
    }
}