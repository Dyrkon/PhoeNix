using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Modules.Queries;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.McpServer.Tools;

[McpServerToolType]
public static class ModuleTemplateTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool]
    [Description(
        "List all NixOS module templates with optional pagination and search. Returns id, name, enabled, type, and supported architectures for each.")]
    public static async Task<string> ListModuleTemplates(
        ISender sender,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Number of items per page")]
        int pageSize = 15,
        [Description("Optional search term to filter by name")]
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListModuleTemplatesQuery(new ListModuleTemplatesRequest(Page: page, PageSize: pageSize,
                Search: search)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Get full details of a module template including its Nix content, editable entry value definitions, tests, and required inputs.")]
    public static async Task<string> GetModuleTemplate(
        ISender sender,
        [Description("Module template ID (GUID)")]
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetModuleTemplateByIdQuery(new ModuleTemplateId(moduleTemplateId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Get the Nix scaffolding (prefix and suffix wrapper) for an existing module template.
                 The scaffolding shows the exact boilerplate that wraps your module content.
                 Write your module code between the prefix and suffix.
                 Also returns test scaffolding for each defined test.
                 """)]
    public static async Task<string> GetModuleScaffolding(
        ISender sender,
        [Description("Module template ID (GUID)")]
        Guid moduleTemplateId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetModuleScaffoldingQuery(new ModuleTemplateId(moduleTemplateId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Preview the Nix scaffolding for a given module type WITHOUT needing an existing template.
                 Use this before creating a module to understand the wrapper format.
                 Module types: System (for NixOS system-level config), Generic (for shared modules).
                 Optionally provide test names to preview test scaffolding too.
                 """)]
    public static async Task<string> GetModuleScaffoldingPreview(
        ISender sender,
        [Description("Module type: System or Generic")]
        string type,
        [Description("Comma-separated list of test names to preview scaffolding for (optional)")]
        string? testNames = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ModuleType>(type, true, out var moduleType))
            throw new InvalidOperationException(
                $"Unknown module type '{type}'. Valid values: {string.Join(", ", Enum.GetNames<ModuleType>())}");

        var names = string.IsNullOrWhiteSpace(testNames)
            ? new List<string>()
            : testNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var result = await sender.Send(
            new GetScaffoldingPreviewQuery(moduleType, names),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Create a new NixOS module template. The 'content' field should contain the inner Nix expression
                 (without the scaffolding wrapper — use get_module_scaffolding_preview to see the wrapper).
                 Use placeholder names like 'myValue' in the content and define them in editableValueTypes.

                 architectures: JSON array of strings, e.g. ["X86_64","Aarch64"]
                 editableValueTypes: JSON array of entry definitions:
                   { "name": string, "placeholder": string, "bindingKind": "None"|"RuntimeValue"|"InputValue",
                     "valueKind": "Text"|"Integer"|"Decimal"|"Enum"|"List",
                     "defaultValue": string|null, "defaultLowerValue": string|null,
                     "integerMin": int|null, "integerMax": int|null,
                     "decimalMin": decimal|null, "decimalMax": decimal|null,
                     "allowLowerValue": bool, "options": string[]|null, "bindingIndex": int|null }
                 tests: JSON array: [{ "id": null, "name": string, "content": string, "variableNames": string[] }]
                 requiredInputs: JSON array: [{ "name": string, "source": string }]
                 """)]
    public static async Task<string> CreateModuleTemplate(
        ISender sender,
        [Description("Template name (must be unique)")]
        string name,
        [Description("Whether this template is enabled")]
        bool enabled,
        [Description("Module type: System or Generic")]
        string type,
        [Description("Inner Nix expression content (without scaffolding wrapper)")]
        string content,
        [Description("JSON array of architecture strings, e.g. [\"X86_64\"]")]
        string architecturesJson,
        [Description("JSON array of editable entry value type definitions (can be empty: [])")]
        string editableValueTypesJson,
        [Description("JSON array of test definitions (can be empty: [])")]
        string testsJson,
        [Description("JSON array of required input definitions (can be empty: [])")]
        string requiredInputsJson,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ModuleType>(type, true, out var moduleType))
            throw new InvalidOperationException(
                $"Unknown module type '{type}'. Valid values: {string.Join(", ", Enum.GetNames<ModuleType>())}");

        var architectures = JsonSerializer.Deserialize<List<string>>(architecturesJson, JsonOptions)
            ?.Select(a =>
            {
                if (!Enum.TryParse<Architecture>(a, true, out var arch))
                    throw new InvalidOperationException($"Unknown architecture '{a}'.");
                return arch;
            })
            .ToList() ?? [];

        var editableValueTypes =
            JsonSerializer.Deserialize<List<ModuleTemplateEntryValueDefinitionModel>>(editableValueTypesJson,
                JsonOptions) ?? [];
        var tests = JsonSerializer.Deserialize<List<ModuleTemplateTestUpsertModel>>(testsJson, JsonOptions) ?? [];
        var requiredInputs =
            JsonSerializer.Deserialize<List<RequiredInputDefinitionModel>>(requiredInputsJson, JsonOptions) ?? [];

        var result = await sender.Send(
            new CreateModuleTemplateCommand(name, enabled, moduleType, content, architectures, editableValueTypes,
                tests, requiredInputs),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Update an existing NixOS module template. All fields are replaced.
                 See create_module_template for parameter format details.
                 """)]
    public static async Task<string> UpdateModuleTemplate(
        ISender sender,
        [Description("Module template ID (GUID)")]
        Guid moduleTemplateId,
        [Description("Template name (must be unique)")]
        string name,
        [Description("Whether this template is enabled")]
        bool enabled,
        [Description("Module type: System or Generic")]
        string type,
        [Description("Inner Nix expression content (without scaffolding wrapper)")]
        string content,
        [Description("JSON array of architecture strings, e.g. [\"X86_64\"]")]
        string architecturesJson,
        [Description("JSON array of editable entry value type definitions (can be empty: [])")]
        string editableValueTypesJson,
        [Description("JSON array of test definitions (can be empty: [])")]
        string testsJson,
        [Description("JSON array of required input definitions (can be empty: [])")]
        string requiredInputsJson,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ModuleType>(type, true, out var moduleType))
            throw new InvalidOperationException(
                $"Unknown module type '{type}'. Valid values: {string.Join(", ", Enum.GetNames<ModuleType>())}");

        var architectures = JsonSerializer.Deserialize<List<string>>(architecturesJson, JsonOptions)
            ?.Select(a =>
            {
                if (!Enum.TryParse<Architecture>(a, true, out var arch))
                    throw new InvalidOperationException($"Unknown architecture '{a}'.");
                return arch;
            })
            .ToList() ?? [];

        var editableValueTypes =
            JsonSerializer.Deserialize<List<ModuleTemplateEntryValueDefinitionModel>>(editableValueTypesJson,
                JsonOptions) ?? [];
        var tests = JsonSerializer.Deserialize<List<ModuleTemplateTestUpsertModel>>(testsJson, JsonOptions) ?? [];
        var requiredInputs =
            JsonSerializer.Deserialize<List<RequiredInputDefinitionModel>>(requiredInputsJson, JsonOptions) ?? [];

        var result = await sender.Send(
            new UpdateModuleTemplateCommand(
                new ModuleTemplateId(moduleTemplateId),
                name, enabled, moduleType, content, architectures, editableValueTypes, tests, requiredInputs),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }
}