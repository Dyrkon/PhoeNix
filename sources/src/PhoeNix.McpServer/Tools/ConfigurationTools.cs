using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Validation;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Configurations.Queries;
using PhoeNix.Application.Models.Validation;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Systems.Commands;
using PhoeNix.Application.Models.Files;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
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
    [Description(
        "Add a module template instance to a specific system within a configuration. Returns the new module value with its ID and entry fields.")]
    public static async Task<string> AddConfigurationSystemModule(
        ISender sender,
        [Description("Configuration ID (GUID)")]
        Guid configurationId,
        [Description("System ID (GUID)")]
        Guid systemId,
        [Description("Module template ID to instantiate (GUID)")]
        Guid moduleTemplateId,
        [Description("Whether this module is enabled in the configuration")]
        bool enabled = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new AddConfigurationSystemModuleCommand(
                new ConfigurationId(configurationId),
                new SystemId(systemId),
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
        [Description("Target architecture: X86Linux, Aarch64Linux, X86Darwin, Aarch64Darwin")]
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

        var moduleTemplates =
            await moduleTemplateRepository.GetByIdsAsync(moduleTemplateIds, configuration.OwnerId, cancellationToken);

        var buildResult = materializer.MaterializeConfiguration(configuration, moduleTemplates);
        if (buildResult.IsFailure)
            throw new InvalidOperationException(buildResult.Error.Description ?? buildResult.Error.Code);

        var folderResult = configurationFilesBuilder.BuildConfigurationFiles(buildResult.Value);
        if (folderResult.IsFailure)
            throw new InvalidOperationException(folderResult.Error.Description ?? folderResult.Error.Code);

        return RenderFolderTree(folderResult.Value);
    }

    [McpServerTool]
    [Description("""
        Validate a NixOS system configuration by running nixos-anywhere --vm-test in a QEMU VM.
        Schedules the validation job, waits for it to complete, and returns pass/fail with details.
        Note: modules with runtime-bound values (e.g. disk paths) will use placeholder values during validation.
        Can take up to 15 minutes for complex configurations.
        On success: returns { state, duration, configurationId, systemId }.
        On failure: throws with the error code and full nix build output to help diagnose configuration issues.
        """)]
    public static async Task<string> ValidateSystem(
        ISender sender,
        IValidationJobTracker jobTracker,
        [Description("Configuration ID (GUID)")] Guid configurationId,
        [Description("System ID within the configuration (GUID)")] Guid systemId,
        [Description("Maximum seconds to wait before giving up (default: 900)")] int timeoutSeconds = 900,
        [Description("Seconds between status checks (default: 15)")] int pollIntervalSeconds = 15,
        CancellationToken cancellationToken = default)
    {
        var configId = new ConfigurationId(configurationId);
        var sysId = new SystemId(systemId);

        var scheduleResult = await sender.Send(
            new ScheduleSystemValidationCommand(configId, sysId),
            cancellationToken);

        if (scheduleResult.IsFailure)
            throw new InvalidOperationException(scheduleResult.Error.Description ?? scheduleResult.Error.Code);

        var key = new SystemValidationKey(configId, sysId);
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), cancellationToken);

            var status = jobTracker.GetSystemStatus(key);

            if (status.State == ValidationJobState.Succeeded)
                return JsonSerializer.Serialize(new
                {
                    state = "Succeeded",
                    duration = status.Duration,
                    configurationId,
                    systemId
                }, JsonOptions);

            if (status.State == ValidationJobState.Failed)
                throw new InvalidOperationException(
                    $"System validation failed.\nError: {status.ErrorCode}\n{status.ErrorMessage}");
        }

        throw new InvalidOperationException(
            $"System validation did not complete within {timeoutSeconds} seconds.");
    }

    [McpServerTool]
    [Description("""
        Validate a module template's Nix checks for a given architecture.
        Schedules the validation job, waits for completion, and returns per-test results.
        Note: modules with runtime-bound values (e.g. disk paths) will use placeholder values during validation.
        On success: returns { state, testResults[] } with each test's outcome.
        On failure: throws with the error code and per-test failure details (expected vs actual values)
        to help diagnose and fix configuration issues.
        """)]
    public static async Task<string> ValidateModule(
        ISender sender,
        IValidationJobTracker jobTracker,
        [Description("Configuration ID (GUID)")] Guid configurationId,
        [Description("Module template ID (GUID)")] Guid moduleTemplateId,
        [Description("Architecture: X86Linux, Aarch64Linux, X86Darwin, Aarch64Darwin")] string architecture,
        [Description("Maximum seconds to wait before giving up (default: 180)")] int timeoutSeconds = 180,
        [Description("Seconds between status checks (default: 10)")] int pollIntervalSeconds = 10,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Architecture>(architecture, true, out var arch))
            throw new InvalidOperationException(
                $"Unknown architecture '{architecture}'. Valid values: X86Linux, Aarch64Linux, X86Darwin, Aarch64Darwin.");

        var configId = new ConfigurationId(configurationId);
        var moduleId = new ModuleTemplateId(moduleTemplateId);

        var scheduleResult = await sender.Send(
            new ScheduleModuleValidationCommand(configId, moduleId, arch),
            cancellationToken);

        if (scheduleResult.IsFailure)
            throw new InvalidOperationException(scheduleResult.Error.Description ?? scheduleResult.Error.Code);

        var key = new ModuleValidationKey(configId, moduleId, arch);
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), cancellationToken);

            var status = jobTracker.GetModuleStatus(key);

            if (status.State == ValidationJobState.Succeeded)
                return JsonSerializer.Serialize(new
                {
                    state = "Succeeded",
                    configurationId,
                    moduleTemplateId,
                    architecture = arch.ToString(),
                    testResults = status.TestResults
                }, JsonOptions);

            if (status.State == ValidationJobState.Failed)
            {
                var failedTests = status.TestResults?
                    .Where(t => !t.IsSuccess)
                    .Select(t => new
                    {
                        t.TestName,
                        t.CheckAttributeName,
                        errors = t.Errors.Select(e => new { e.Name, e.Expected, actual = e.Result })
                    })
                    .ToList();

                var detail = failedTests?.Count > 0
                    ? JsonSerializer.Serialize(failedTests, JsonOptions)
                    : status.ErrorMessage;

                throw new InvalidOperationException(
                    $"Module validation failed.\nError: {status.ErrorCode}\n{detail}");
            }
        }

        throw new InvalidOperationException(
            $"Module validation did not complete within {timeoutSeconds} seconds.");
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