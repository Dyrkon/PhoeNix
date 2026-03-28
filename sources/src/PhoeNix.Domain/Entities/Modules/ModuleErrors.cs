using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public static class ModuleErrors
{
    public static Error NotFound(ModuleTemplateId moduleTemplateId)
    {
        return new Error("Modules.NotFound", $"Module template '{moduleTemplateId}' was not found.");
    }

    public static Error NameAlreadyExists(string name)
    {
        return new Error("Modules.NameAlreadyExists", $"Module template with name '{name}' already exists.");
    }
}