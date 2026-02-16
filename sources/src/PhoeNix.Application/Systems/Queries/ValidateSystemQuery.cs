using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Tests;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Systems.Queries;

public record ValidateSystemQuery(ConfigurationId ConfigurationId, SystemId SystemId) : IQuery<SystemTestResponse>;

internal sealed class ValidateSystemQueryHandler(
    INixTestRunner nixTestRunner,
    IConfigurationRepository configurationRepository,
    IFileSystemService fileSystemService)
    : IQueryHandler<ValidateSystemQuery, SystemTestResponse>
{
    public async Task<Result<SystemTestResponse>> Handle(ValidateSystemQuery query, CancellationToken cancellationToken)
    {
        return await configurationRepository
            .GetByIdAsync(query.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error("", $"Configuration {query.ConfigurationId} not found!"))
            .Ensure(conf => conf.SystemSpecifications.Any(s => s.Id == query.SystemId),
                conf => new Error("", $"Configuration {conf.Title} does not contain system {query.SystemId.Value}"))
            .Bind(x =>
                fileSystemService
                    .GetRootFolder()
                    .Bind(path =>
                    {
                        var system = x.SystemSpecifications.First(s => s.Id == query.SystemId);
                        return nixTestRunner.RunSystemTest(
                            system.Id,
                            system.Architecture, $"{path}/{x.Id.Value}", cancellationToken);
                    }));
    }
}