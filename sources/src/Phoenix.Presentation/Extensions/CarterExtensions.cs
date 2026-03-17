using System.Reflection;
using Carter;
using FluentValidation;

namespace Phoenix.Presentation.Extensions;

public static class CarterConfiguratorExtensions
{
    public static CarterConfigurator WithValidatorsFromAssembly(
        this CarterConfigurator configurator,
        Assembly assembly)
    {
        var validatorTypes = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => typeof(IValidator).IsAssignableFrom(t));

        foreach (var vt in validatorTypes) configurator.WithValidators(vt);

        return configurator;
    }
}