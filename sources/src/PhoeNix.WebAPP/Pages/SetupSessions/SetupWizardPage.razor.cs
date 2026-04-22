using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Machines;
using PhoeNix.Contracts.Setup;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Pages.SetupSessions;

public partial class SetupWizardPage : ComponentBase
{
    [Inject] private ISetupApiClient SetupApiClient { get; set; } = null!;
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public SetupSessionsState SetupSessionsState { get; set; } = null!;

    private int _activeStep;

    private bool _sessionStarting = true;
    private string? _sessionStartError;
    private Guid? _sessionId;
    private Task? _sessionStartTask;

    private bool _machinesLoading;
    private bool _configurationsLoading;
    private bool _configurationsDetailLoading;
    private bool _registering;
    private string? _registerError;
    private string _registerStatus = string.Empty;

    private List<MachineSelectionRow> _availableMachines = [];
    private HashSet<MachineSelectionRow> _selectedMachineItems = [];

    private List<ConfigurationSelectionRow> _availableConfigurations = [];
    private HashSet<ConfigurationSelectionRow> _selectedConfigurationItems = [];

    private readonly Dictionary<Guid, ConfigurationWithRevisionsResponse> _configurationDetailsCache = new();
    private List<SystemOption> _availableSystems = [];
    private List<MachineAssignment> _machineAssignments = [];

    private bool CanAdvance => _activeStep switch
    {
        0 => _selectedMachineItems.Count > 0,
        1 => _selectedConfigurationItems.Count > 0,
        2 => _machineAssignments.All(a => a.SelectedSystem is not null),
        3 => true,
        _ => false
    };

    protected override void OnInitialized()
    {
        _sessionStartTask = StartSessionAsync();
        _ = LoadMachinesAsync();
        _ = LoadConfigurationsAsync();
    }

    private async Task StartSessionAsync()
    {
        _sessionStarting = true;

        var result = await SetupApiClient.StartSessionAsync();

        if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value))
        {
            _sessionStartError = result.Error?.Description ?? "Unknown error.";
            _sessionStarting = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        _sessionId = Guid.Parse(result.Value);
        _sessionStarting = false;
        SetupSessionsState.StartPolling();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadMachinesAsync()
    {
        _machinesLoading = true;

        var response = await MachinesApiClient.GetMachinesAsync(
            new ListMachinesRequest(PageSize: 100));

        if (response.IsFailure || response.Value is null)
            Snackbar.Add("Failed to load machines.", Severity.Error);
        else
            _availableMachines = response.Value.Items
                .Select(m => new MachineSelectionRow(m.Id, m.Title, m.MacAddress, m.Architecture))
                .ToList();

        _machinesLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadConfigurationsAsync()
    {
        _configurationsLoading = true;

        var response = await ConfigurationsApiClient.GetConfigurationsAsync(
            new ListConfigurationsRequest(PageSize: 100));

        if (response.IsFailure || response.Value is null)
            Snackbar.Add("Failed to load configurations.", Severity.Error);
        else
            _availableConfigurations = response.Value.Items
                .Select(c => new ConfigurationSelectionRow(c.Id, c.Title, c.Description))
                .ToList();

        _configurationsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadSelectedConfigurationDetailsAsync()
    {
        _configurationsDetailLoading = true;

        var uncachedIds = _selectedConfigurationItems
            .Where(c => !_configurationDetailsCache.ContainsKey(c.Id))
            .Select(c => c.Id)
            .ToList();

        var results = await Task.WhenAll(
            uncachedIds.Select(id => ConfigurationsApiClient.GetConfigurationAsync(id)));

        foreach (var result in results)
        {
            if (result.IsFailure || result.Value is null)
            {
                Snackbar.Add("Failed to load one or more configuration details.", Severity.Error);
                continue;
            }

            _configurationDetailsCache[result.Value.Id] = result.Value;
        }

        var multipleConfigurations = _selectedConfigurationItems.Count > 1;

        _availableSystems = _selectedConfigurationItems
            .Where(c => _configurationDetailsCache.ContainsKey(c.Id))
            .SelectMany(c => _configurationDetailsCache[c.Id].Systems
                .Select(s => new SystemOption(
                    s.Id,
                    s.Name,
                    c.Id,
                    c.Title,
                    multipleConfigurations ? $"{s.Name} ({c.Title})" : s.Name)))
            .ToList();

        _configurationsDetailLoading = false;
    }

    private async Task NextStepAsync(MudStepper stepper)
    {
        if (_activeStep == 0)
            BuildAssignments();

        if (_activeStep == 1)
            await LoadSelectedConfigurationDetailsAsync();

        await stepper.NextStepAsync();
    }

    private void BuildAssignments()
    {
        var existingById = _machineAssignments.ToDictionary(a => a.MachineId);

        _machineAssignments = _selectedMachineItems
            .Select(m => existingById.TryGetValue(m.Id, out var existing)
                ? existing
                : new MachineAssignment(m.Id, m.Title, m.MacAddress))
            .ToList();
    }

    private void OnSystemSelected(MachineAssignment assignment, SystemOption? system)
    {
        assignment.SelectedSystem = system;
        StateHasChanged();
    }

    private async Task RegisterMachinesAsync()
    {
        if (_sessionId is null)
        {
            if (_sessionStartTask is not null)
                await _sessionStartTask;

            if (_sessionId is null)
            {
                Snackbar.Add("Session failed to start. Cannot register machines.", Severity.Error);
                return;
            }
        }

        _registering = true;
        _registerError = null;

        foreach (var assignment in _machineAssignments)
        {
            _registerStatus = $"Registering '{assignment.MachineTitle}'...";
            StateHasChanged();

            var result = await SetupApiClient.StartMachineSetupAsync(
                _sessionId.Value,
                assignment.MachineId,
                new StartMachineSetupRequest(
                    assignment.SelectedSystem!.ConfigurationId,
                    assignment.SelectedSystem.SystemId));

            if (result.IsFailure)
            {
                _registerError = $"Failed to register '{assignment.MachineTitle}': {result.Error?.Description}";
                _registering = false;
                return;
            }
        }

        NavigateToSession();
    }

    private void NavigateToSession()
    {
        if (_sessionId.HasValue)
            NavigationManager.NavigateToSetupSessionDetail(_sessionId.Value);
    }

    internal sealed record MachineSelectionRow(
        Guid Id,
        string Title,
        string MacAddress,
        Architecture Architecture);

    internal sealed record ConfigurationSelectionRow(
        Guid Id,
        string Title,
        string Description);

    internal sealed record SystemOption(
        Guid SystemId,
        string SystemName,
        Guid ConfigurationId,
        string ConfigurationTitle,
        string DisplayName);

    internal sealed class MachineAssignment(Guid machineId, string machineTitle, string macAddress)
    {
        public Guid MachineId { get; } = machineId;
        public string MachineTitle { get; } = machineTitle;
        public string MacAddress { get; } = macAddress;
        public SystemOption? SelectedSystem { get; set; }
    }
}