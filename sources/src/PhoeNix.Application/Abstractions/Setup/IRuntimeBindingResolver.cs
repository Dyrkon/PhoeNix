using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface IRuntimeBindingResolver
{
    Result<Configuration> ApplyBindings(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        SetupTarget target);
}