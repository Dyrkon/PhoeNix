using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Components.Authentication;

public partial class RegistrationForm : ComponentBase
{
    [Inject] private IAuthenticationApiClient AuthenticationApiClient { get; set; } = null!;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [CascadingParameter] public UserState UserState { get; set; } = null!;

    private readonly RegistrationModel _model = new();

    private bool _isSubmitting;

    private string? _errorMessage;

    private string SubmitButtonText => _isSubmitting ? "Creating account..." : "Register";

    private async Task SubmitAsync()
    {
        if (_isSubmitting)
            return;

        _isSubmitting = true;
        _errorMessage = null;

        var result = await AuthenticationApiClient.RegisterAsync(
            new UserRegisterRequest(_model.Name, _model.Password));

        _isSubmitting = false;

        if (result.IsFailure || result.Value is null)
        {
            _errorMessage = result.Error?.Description ?? "Unable to register.";
            return;
        }

        UserState.SetCurrentUser(result.Value);
        NavigationManager.NavigateTo("/");
    }

    private sealed class RegistrationModel
    {
        [Required]
        [StringLength(64, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required] [MinLength(8)] public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}