using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;

namespace PhoeNix.WebAPP.Components.Templates;

public partial class EntryDefinitionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public EntryDefinitionModel Model { get; set; } = new();
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public IReadOnlyList<string> ExistingNames { get; set; } = [];
    [Parameter] public IReadOnlyList<string> ExistingPlaceholders { get; set; } = [];

    private MudForm? _form;
    private bool _isSubmitting;
    private string _newOption = string.Empty;
    private string _newListItem = string.Empty;

    private void AddOption()
    {
        if (string.IsNullOrWhiteSpace(_newOption))
            return;

        var trimmed = _newOption.Trim();
        if (!Model.Options.Contains(trimmed))
        {
            Model.Options.Add(trimmed);
        }

        _newOption = string.Empty;
    }

    private void RemoveOption(string option)
    {
        Model.Options.Remove(option);
    }

    private void AddListItem()
    {
        if (string.IsNullOrWhiteSpace(_newListItem))
            return;

        var trimmed = _newListItem.Trim();
        Model.DefaultListItems.Add(trimmed);
        _newListItem = string.Empty;
    }

    private void RemoveListItem(string item)
    {
        Model.DefaultListItems.Remove(item);
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        var validationErrors = ValidateModel();
        if (validationErrors.Count > 0)
        {
            return;
        }

        _isSubmitting = true;

        try
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private List<string> ValidateModel()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            errors.Add("Name is required.");
        }
        else if (!IsEditMode && ExistingNames.Contains(Model.Name, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add("Name must be unique.");
        }

        if (string.IsNullOrWhiteSpace(Model.Placeholder))
        {
            errors.Add("Placeholder is required.");
        }
        else if (!IsEditMode && ExistingPlaceholders.Contains(Model.Placeholder, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add("Placeholder must be unique.");
        }

        switch (Model.ValueKind)
        {
            case EntryValueKind.IntegerRange:
                if (!Model.IntegerMin.HasValue || !Model.IntegerMax.HasValue)
                {
                    errors.Add("Integer range requires both min and max values.");
                }
                else if (Model.IntegerMax < Model.IntegerMin)
                {
                    errors.Add("Integer max must be greater than or equal to min.");
                }

                break;

            case EntryValueKind.DecimalRange:
                if (!Model.DecimalMin.HasValue || !Model.DecimalMax.HasValue)
                {
                    errors.Add("Decimal range requires both min and max values.");
                }
                else if (Model.DecimalMax < Model.DecimalMin)
                {
                    errors.Add("Decimal max must be greater than or equal to min.");
                }

                break;

            case EntryValueKind.SingleChoice:
                if (Model.Options.Count == 0)
                {
                    errors.Add("Single choice requires at least one option.");
                }

                break;
        }

        if (Model.BindingKind == EntryBindingKind.RankedDiskCandidate && !Model.BindingIndex.HasValue)
        {
            errors.Add("Binding index is required for ranked disk candidate.");
        }

        return errors;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    public sealed class EntryDefinitionModel
    {
        public string Name { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public EntryBindingKind BindingKind { get; set; } = EntryBindingKind.UserProvided;
        public EntryValueKind ValueKind { get; set; } = EntryValueKind.Text;
        public int? IntegerMin { get; set; }
        public int? IntegerMax { get; set; }
        public decimal? DecimalMin { get; set; }
        public decimal? DecimalMax { get; set; }
        public List<string> Options { get; set; } = [];
        public List<string> DefaultListItems { get; set; } = [];
        public bool AllowLowerValue { get; set; }
        public string? DefaultValue { get; set; }
        public string? DefaultLowerValue { get; set; }
        public int? BindingIndex { get; set; }

        public EntryDefinitionModel Clone()
        {
            return new EntryDefinitionModel
            {
                Name = Name,
                Placeholder = Placeholder,
                BindingKind = BindingKind,
                ValueKind = ValueKind,
                IntegerMin = IntegerMin,
                IntegerMax = IntegerMax,
                DecimalMin = DecimalMin,
                DecimalMax = DecimalMax,
                Options = [..Options],
                DefaultListItems = [..DefaultListItems],
                AllowLowerValue = AllowLowerValue,
                DefaultValue = DefaultValue,
                DefaultLowerValue = DefaultLowerValue,
                BindingIndex = BindingIndex
            };
        }
    }
}
