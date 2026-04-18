using Microsoft.Playwright;

namespace PhoeNix.WebAPP.Tests;

public class PhoeNixTestBase : PageTest
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST_BASE_URL")
        ?? "http://localhost:5269";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        };
    }
}