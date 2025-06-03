using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class Module : AggregateRoot<ModuleId>
{
    private readonly List<Architecture> _supportedArchitectures = new();
    private readonly List<EntryValue> _editableValues = new();

    private Module(ModuleId id) : base(id)
    {
    }

    public string Name { get; private set; }

    public bool Enabled { get; private set; }

    public ModuleType Type { get; private set; }

    public string Content { get; private set; }

    public IReadOnlyList<EntryValue> EditableValues => _editableValues;

    public Result ChangeContent(string content, List<EntryValue> entries)
    {
        foreach (var entryValue in entries.Where(entryValue => !content.Contains(entryValue.Name)))
            return Result.Failure(new Error("", $"Name for value {entryValue.Name} is not present"));

        Content = content;
        _editableValues.Clear();
        _editableValues.AddRange(entries);

        return Result.Success();
    }

    public Result AddEntry(EntryValue entry)
    {
        if (_editableValues.Any(v => v.Id == entry.Id))
            return Result.Failure(new Error("", $"Can't add editable value twice"));

        if (!Content.Contains(entry.Name))
            return Result.Failure(new Error("", $"Name of the entry {entry.Name} is not present in content"));

        _editableValues.Add(entry);

        return Result.Success();
    }

    public Result RemoveEntry(EntryValueId entryId)
    {
        var removed = _editableValues.RemoveAll(v => v.Id == entryId);
        return removed == 0 ? Result.Failure(new Error("", $"Entry not present in module")) : Result.Success();
    }

    public Result Disable()
    {
        if (Enabled == false)
            return Result.Failure(new Error("", $"Module {Name} is already disabled"));
        Enabled = false;
        return Result.Success();
    }

    public Result Enable()
    {
        if (Enabled == true)
            return Result.Failure(new Error("", $"Module {Name} is already enabled"));
        Enabled = true;
        return Result.Success();
    }

    public Result EditModule(string newName)
    {
        if (newName == string.Empty)
            return Result.Failure(new Error("", "Module name can't be empty"));

        Name = newName;
        return Result.Success();
    }

    public Result AddArchitectureSupport(Architecture architecture)
    {
        if (_supportedArchitectures.Contains(architecture))
            return Result.Failure(new Error("", $"Can't add already supported architecture {architecture}"));

        _supportedArchitectures.Add(architecture);
        return Result.Success();
    }

    public Result AddArchitecturesSupport(IEnumerable<Architecture> architectures)
    {
        if (_supportedArchitectures.Any(architectures.Contains))
            return Result.Failure(new Error("", "Can't add already supported architectures"));

        _supportedArchitectures.AddRange(architectures);
        return Result.Success();
    }

    public Result RemoveArchitectureSupport(Architecture architecture)
    {
        var removed = _supportedArchitectures.RemoveAll(a => a == architecture);
        if (removed == 0)
            return Result.Failure(Error.ValueNotFound);

        return Result.Success();
    }

    public static Result<Module> Create(ModuleId id, string name, bool enabled, ModuleType type,
        List<Architecture> architectures)
    {
        if (name == string.Empty)
            return Result.Failure<Module>(new Error("", "Modules name can't be empty"));

        if (architectures.Count == 0)
            return Result.Failure<Module>(new Error("", "Module has to support at least one architecture"));

        var newModule = new Module(id)
        {
            Name = name,
            Enabled = enabled,
            Type = type,
            Content = string.Empty
        };
        var result = newModule.AddArchitecturesSupport(architectures);
        return result.IsFailure ? Result.Failure<Module>(result.Error) : newModule;
    }

    public IReadOnlyList<Architecture> SupportedArchitectures => _supportedArchitectures;

    public Result<ModuleBuildResult> Build(string moduleValuesName = "values.nix")
    {
        var inputs = "{ ";
        var outputContent = Content;
        foreach (var value in EditableValues)
        {
            inputs += $"{value.Name} = {value.Value};";
            outputContent = outputContent.Replace(value.Name, $"args.{value.Name}");
        }

        inputs += " }";
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();
        var moduleContent =
            $"{{ config, pkgs, ... }}: let\n args = import {inputsLocationPlaceholder}/{moduleValuesName}; \nin {{ {outputContent} }}";

        return new ModuleBuildResult(Name, moduleContent, inputs, moduleValuesName, inputsLocationPlaceholder);
    }
}