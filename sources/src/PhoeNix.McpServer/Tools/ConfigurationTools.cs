using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Configurations.Queries;
using PhoeNix.Application.Models.Files;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.McpServer.Tools;

[McpServerToolType]
public static class ConfigurationTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool]
    [Description(
        "List all NixOS configurations with optional pagination and search. Returns id, title, description for each.")]
    public static async Task<string> ListConfigurations(
        ISender sender,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Number of items per page")]
        int pageSize = 15,
        [Description("Optional search term to filter by title")]
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListConfigurationsQuery(new ListConfigurationsRequest(page, pageSize, search)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Get full details of a NixOS configuration by ID, including all inputs, modules (with entry values), and systems.")]
    public static async Task<string> GetConfiguration(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetConfigurationByIdQuery(new ConfigurationId(configurationId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Create a new NixOS configuration with a title and description. Returns the created configuration.")]
    public static async Task<string> CreateConfiguration(
        ISender sender,
        [Description("Human-readable title for the configuration")]
        string title,
        [Description("Description of what this configuration is for")]
        string description,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new CreateConfigurationCommand(title, description),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Update title and description of an existing NixOS configuration. Returns the updated configuration.")]
    public static async Task<string> UpdateConfiguration(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        [Description("New title")] string title,
        [Description("New description")] string description,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new UpdateConfigurationCommand(new ConfigurationId(configurationId), title, description),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Delete a NixOS configuration by ID.")]
    public static async Task DeleteConfiguration(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new RemoveConfigurationCommand(new ConfigurationId(configurationId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);
    }

    [McpServerTool]
    [Description(
        "Add a module template instance to a configuration. Returns the new module value with its ID and entry fields.")]
    public static async Task<string> AddConfigurationModule(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        [Description("Module template ID to instantiate (GUID)")]
        Guid moduleTemplateId,
        [Description("Whether this module is enabled in the configuration")]
        bool enabled = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new AddConfigurationModuleCommand(
                new ConfigurationId(configurationId),
                new ModuleTemplateId(moduleTemplateId),
                enabled),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Update a module instance within a configuration. Supply entry values as JSON array.
                 Each entry: { "name": string, "placeholder": string, "kind": "Text"|"Integer"|"Decimal"|"Enum"|"List",
                 "textValue": string|null, "integerUpperValue": int|null, "integerLowerValue": int|null,
                 "decimalUpperValue": decimal|null, "decimalLowerValue": decimal|null,
                 "selectedValue": string|null, "listItems": string[]|null }
                 Returns the updated module value.
                 """)]
    public static async Task<string> UpdateConfigurationModule(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        [Description("Module value ID to update (GUID)")]
        Guid moduleValueId,
        [Description("Whether this module is enabled")]
        bool enabled,
        [Description("JSON array of ModuleEntryValueUpsertModel entries")]
        string entriesJson,
        CancellationToken cancellationToken = default)
    {
        var entries = JsonSerializer.Deserialize<List<ModuleEntryValueUpsertModel>>(entriesJson, JsonOptions)
                      ?? throw new InvalidOperationException("Failed to deserialize entries JSON.");

        var result = await sender.Send(
            new UpdateConfigurationModuleCommand(
                new ConfigurationId(configurationId),
                new ModuleValueId(moduleValueId),
                enabled,
                entries),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Add a system target to a configuration. A system represents a specific machine architecture variant (e.g. x86_64-linux). Returns the new system.")]
    public static async Task<string> AddConfigurationSystem(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        [Description("Name for the system (e.g. 'homeserver', 'laptop')")]
        string name,
        [Description("Target architecture: X86_64, Aarch64, Armv7, RiscV64")]
        string architecture,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Architecture>(architecture, true, out var arch))
            throw new InvalidOperationException(
                $"Unknown architecture '{architecture}'. Valid values: {string.Join(", ", Enum.GetNames<Architecture>())}");

        var result = await sender.Send(
            new AddConfigurationSystemCommand(new ConfigurationId(configurationId), name, arch),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Render a NixOS configuration into its Nix flake file tree. Returns all generated files (flake.nix, module files, values files) so you can see exactly what Nix code will be deployed.")]
    public static async Task<string> PreviewConfigurationNix(
        IConfigurationRepository configurationRepository,
        IModuleTemplateRepository moduleTemplateRepository,
        INixBuildMaterializer materializer,
        IConfigurationFilesBuilder configurationFilesBuilder,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            new ConfigurationId(configurationId), cancellationToken);

        if (configuration is null)
            throw new InvalidOperationException($"Configuration '{configurationId}' not found.");

        var moduleTemplateIds = configuration.Modules
            .Select(m => m.ModuleTemplateId)
            .Concat(configuration.SystemSpecifications.SelectMany(s => s.Modules).Select(m => m.ModuleTemplateId))
            .Distinct()
            .ToList();

        var moduleTemplates = await moduleTemplateRepository.GetByIdsAsync(moduleTemplateIds, cancellationToken);

        var buildResult = materializer.MaterializeConfiguration(configuration, moduleTemplates);
        if (buildResult.IsFailure)
            throw new InvalidOperationException(buildResult.Error.Description ?? buildResult.Error.Code);

        var folderResult = configurationFilesBuilder.BuildConfigurationFiles(buildResult.Value);
        if (folderResult.IsFailure)
            throw new InvalidOperationException(folderResult.Error.Description ?? folderResult.Error.Code);

        return RenderFolderTree(folderResult.Value);
    }

    private static string RenderFolderTree(Folder folder, string indent = "")
    {
        var sb = new System.Text.StringBuilder();
        foreach (var file in folder.Files)
            if (file is TextFile textFile)
            {
                sb.AppendLine($"{indent}=== {textFile.Name} ===");
                sb.AppendLine(textFile.Content);
            }
            else if (file is Folder subFolder)
            {
                sb.AppendLine($"{indent}--- {subFolder.Name}/ ---");
                sb.Append(RenderFolderTree(subFolder, indent + "  "));
            }

        return sb.ToString();
    }
}