using Microsoft.AspNetCore.Components;

namespace PhoeNix.WebAPP.Extensions;

public static class NavigationExtensions
{
    public static void NavigateToHome(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo("/");
    }

    public static void NavigateToLogin(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo("/login");
    }

    public static void NavigateToRegister(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo("/register");
    }

    public static void NavigateToMachineIndex(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo($"/");
    }

    public static void NavigateToMachineDetail(this NavigationManager navigationManager, Guid machineId)
    {
        navigationManager.NavigateTo($"/machines/{machineId}");
    }

    public static void NavigateToConfigurationIndex(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo($"/configurations");
    }

    public static void NavigateToConfigurationDetail(this NavigationManager navigationManager, Guid configurationId)
    {
        navigationManager.NavigateTo($"/configurations/{configurationId}");
    }

    public static void NavigateToSetupSessions(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo($"/setup");
    }

    public static void NavigateToNewSetupSessions(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo($"/setup/new");
    }

    public static void NavigateToSetupSessionDetail(this NavigationManager navigationManager, Guid setupSessionId)
    {
        navigationManager.NavigateTo($"/setup/{setupSessionId}");
    }

    public static void NavigateToTemplates(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo($"/templates");
    }

    public static void NavigateToTemplatesDetail(this NavigationManager navigationManager, Guid templateId)
    {
        navigationManager.NavigateTo($"/templates/{templateId}");
    }

    public static void NavigateToTemplateCreator(this NavigationManager navigationManager)
    {
        navigationManager.NavigateTo("/templates/new");
    }
}