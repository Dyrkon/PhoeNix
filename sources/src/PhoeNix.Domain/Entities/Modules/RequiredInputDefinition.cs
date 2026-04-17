namespace PhoeNix.Domain.Entities.Modules;

public sealed class RequiredInputDefinition
{
    private RequiredInputDefinition()
    {
    }

    public ModuleTemplateId ModuleTemplateId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;

    public static RequiredInputDefinition Create(ModuleTemplateId moduleTemplateId, string name, string source)
    {
        return new RequiredInputDefinition
        {
            ModuleTemplateId = moduleTemplateId,
            Name = name,
            Source = source
        };
    }
}
