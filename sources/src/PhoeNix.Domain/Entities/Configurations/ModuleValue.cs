using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ModuleValue : Entity<ModuleValueId>
{
    private readonly List<EntryValue> _editableValues = new();
    public ConfigurationId ConfigurationId { get; private set; }
    public ModuleTemplateId ModuleTemplateId { get; private set; }
    public SystemId SystemId { get; private set; }

    private ModuleValue(ModuleValueId id) : base(id)
    {
    }

    public bool Enabled { get; private set; }

    public IReadOnlyList<EntryValue> EditableValues => _editableValues;

    public Result Enable()
    {
        if (Enabled == true)
            return Result.Failure(new Error("", $"Module {Id.ToString()} is already enabled"));
        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (Enabled == false)
            return Result.Failure(new Error("", $"Module {Id.ToString()} is already disabled"));
        Enabled = false;
        return Result.Success();
    }

    public Result ChangeValues(List<EntryValue> entries, string? content = null)
    {
        if (content != null)
        {
            foreach (var entryValue in entries.Where(entryValue => !content.Contains(entryValue.Name)))
                return Result.Failure(new Error("", $"Name for value {entryValue.Name} is not present"));
        }
        else
        {
            if (entries.Any(entry => _editableValues.First(i => i.Id == entry.Id).Name != entry.Name))
                return Result.Failure(new Error("", $"Changing module value names requires module template"));
        }

        _editableValues.Clear();
        _editableValues.AddRange(entries);

        return Result.Success();
    }

    public static Result<ModuleValue> Create(ModuleValueId moduleValueId, ModuleTemplateId moduleTemplateId,
        bool enabled = true)
    {
        return new ModuleValue(moduleValueId)
        {
            ModuleTemplateId = moduleTemplateId,
            Enabled = enabled
        };
    }

    public Result<ModuleBuildResult> Build(ModuleTemplate template, string moduleValuesName = "values")
    {
        var inputs = "{ ";
        var outputContent = template.Content;
        foreach (var value in EditableValues)
        {
            inputs += $"{value.Name} = {value.Value};";
            outputContent = outputContent.Replace(value.Name, $"args.{value.Name}");
        }

        inputs += " }";
        var config = template.Type == ModuleType.System ? "config, " : "";
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();
        var moduleContent =
            $"{{ inputs, pkgs, lib, system, {config}... }}: let\n args = import {inputsLocationPlaceholder}/{moduleValuesName}.nix; \nin {{ {outputContent} }}";

        var moduleTests = template.Tests.Select(t => t.Build()).ToList();
        if (moduleTests.Any(i => i.IsFailure))
            return Result.Failure<ModuleBuildResult>(
                new Error("", $"Failed to build tests for module {template.Name}."));

        return new ModuleBuildResult(template.Id, template.Name, moduleContent, inputs, moduleValuesName,
            inputsLocationPlaceholder,
            moduleTests.Select(t => t.Value).ToList());
    }
}