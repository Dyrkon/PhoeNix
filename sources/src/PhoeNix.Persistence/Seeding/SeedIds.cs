using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Persistence.Seeding;

internal static class SeedIds
{
    public static readonly ModuleTemplateId MinimalBaseTemplate = new(new Guid("10000000-0000-0000-0000-000000000001"));

    public static readonly ModuleTemplateId
        DiskoEfiExt4Template = new(new Guid("10000000-0000-0000-0000-000000000002"));

    public static readonly ModuleTemplateId CallbackTemplate = new(new Guid("10000000-0000-0000-0000-000000000003"));
    public static readonly ModuleTemplateId PrometheusTemplate = new(new Guid("10000000-0000-0000-0000-000000000004"));

    public static readonly ModuleTemplateId TimezoneSyncTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000004"));

    public static readonly ModuleTemplateId NixFlakeSettingsTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000005"));

    public static readonly ModuleTemplateId NixBuildOptimisationTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000006"));

    public static readonly ConfigurationId ExampleConfiguration = new(new Guid("20000000-0000-0000-0000-000000000001"));

    public static readonly SystemId ExampleSystem = new(new Guid("30000000-0000-0000-0000-000000000001"));

    public static readonly AppSettingsId DefaultAppSettings = new(new Guid("50000000-0000-0000-0000-000000000001"));
}