using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.Contracts.Auth;
using PhoeNix.WebAPP.Extensions;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Components.Authentication;

public partial class LoginForm : ComponentBase
{
    [Inject] private IAuthenticationApiClient AuthenticationApiClient { get; set; } = null!;

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private readonly LoginModel _model = new();

    private bool _isSubmitting;

    private string? _errorMessage;

    private string SubmitButtonText => _isSubmitting ? "Signing in..." : "Sign in";

    private async Task SubmitAsync()
    {
        if (_isSubmitting)
            return;

        _isSubmitting = true;
        _errorMessage = null;

        var result = await AuthenticationApiClient.LoginAsync(
            new UserLoginRequest(_model.Name, _model.Password));

        _isSubmitting = false;

        if (result.IsFailure || result.Value is null)
        {
            _errorMessage = result.Error?.Description ?? "Unable to sign in.";
            return;
        }

        NavigationManager.NavigateTo(AppRoutes.Home);
    }

    private sealed class LoginModel
    {
        [Required]
        [StringLength(64, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required] [MinLength(8)] public string Password { get; set; } = string.Empty;
    }
}