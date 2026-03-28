using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Deployment;

public interface IDeploymentBindingResolver
{
    Result<Configuration> ApplyBindings(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        DeploymentSnapshot deploymentSnapshot);
}