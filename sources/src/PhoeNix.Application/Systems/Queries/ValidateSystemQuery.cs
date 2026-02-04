using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Systems.Queries;

public record ValidateSystemQuery(ConfigurationId ConfigurationId, SystemId SystemId) : IQuery<bool>;

internal sealed class ValidateSystemQueryHandler(
    IConfigurationTestRunnerService configurationTestRunnerService,
    IConfigurationRepository configurationRepository,
    ISystemRepository systemRepository,
    IFileSystemService fileSystemService)
    : IQueryHandler<ValidateSystemQuery, bool>
{
    public async Task<Result<bool>> Handle(ValidateSystemQuery query, CancellationToken cancellationToken)
    {
        var config = await configurationRepository.GetByIdAsync(query.ConfigurationId, cancellationToken);

        if (config is null)
            return Result.Failure<bool>(new Error("", $"Configuration {query.ConfigurationId} not found!"));

        var system = await systemRepository.GetByIdAsync(query.SystemId, cancellationToken);

        if (system is null) return Result.Failure<bool>(new Error("", $"System {query.SystemId} not found!"));

        if (config.Systems.All(s => s.SystemId != system.Id))
            return Result.Failure<bool>(
                new Error("", $"System {query.SystemId} is not in configuration {config.Title}"));

        return fileSystemService.GetRootFolder().Bind(path => configurationTestRunnerService.RunSystemTest(system.Id,
            system.Architecture,
            $"{path}/{config.Id}"));
    }
}