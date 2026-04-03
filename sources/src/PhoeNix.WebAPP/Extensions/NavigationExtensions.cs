using Microsoft.AspNetCore.Components;

namespace PhoeNix.WebAPP.Extensions;

public static class NavigationExtensions
{
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
}