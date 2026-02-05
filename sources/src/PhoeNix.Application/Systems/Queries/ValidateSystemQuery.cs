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
        return await configurationRepository
            .GetByIdAsync(query.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("", $"Configuration {query.ConfigurationId} not found!"))
            .Bind(config =>
                systemRepository
                    .GetByIdAsync(query.SystemId, cancellationToken)
                    .EnsureNotNull(new Error("", $"System {query.SystemId} not found!"))
                    .Bind(system => Result.Success((config, system))))
            .Ensure(
                x => x.config.Systems.Any(s => s.SystemId == x.system.Id),
                x => new Error("", $"System {query.SystemId} is not in configuration {x.config.Title}"))
            .Bind(x =>
                fileSystemService
                    .GetRootFolder()
                    .Bind(path =>
                        configurationTestRunnerService.RunSystemTest(
                            x.system.Id.ToStringWithPrefix(),
                            x.system.Architecture,
                            $"{path}/{x.config.Id.Value}")));
    }
}