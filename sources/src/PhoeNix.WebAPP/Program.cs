using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PhoeNix.WebAPP.ApiClient;

namespace PhoeNix.WebAPP;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();

        var apiBaseAddress = "https://localhost:7031";
        var httpBaseAddress = string.IsNullOrWhiteSpace(apiBaseAddress)
            ? builder.HostEnvironment.BaseAddress
            : apiBaseAddress;

        builder.Services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri(httpBaseAddress, UriKind.Absolute)
        });

        builder.Services.AddPhoeNixApiClients();

        await builder.Build().RunAsync();
    }
}