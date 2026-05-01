using PhoeNix.Domain.Entities.Modules;

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

    public static readonly ModuleTemplateId GnomeTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000007"));

    public static readonly ModuleTemplateId KdeTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000008"));

    public static readonly ModuleTemplateId PhoeNixServiceTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000009"));

    public static readonly ModuleTemplateId NcpsCacheServerTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000010"));

    public static readonly ModuleTemplateId NcpsCacheClientTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000011"));

    public static readonly ModuleTemplateId SystemHardeningTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000012"));

    public static readonly ModuleTemplateId ItSupportTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000013"));

    public static readonly ModuleTemplateId AmdGpuTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000014"));

    public static readonly ModuleTemplateId NvidiaGpuTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000015"));

    public static readonly ModuleTemplateId DiskoEfiBtrfsTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000016"));

    public static readonly ModuleTemplateId DiskoEfiLuksExt4Template =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000017"));

    public static readonly ModuleTemplateId DiskoEfiZfsTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000018"));

    public static readonly ModuleTemplateId DiskoSsdHddTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000019"));

    public static readonly ModuleTemplateId AdminUserTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000020"));

    public static readonly ModuleTemplateId RegularUserTemplate =
        new(new Guid("a1b2c3d4-0001-0002-0003-000000000021"));

}