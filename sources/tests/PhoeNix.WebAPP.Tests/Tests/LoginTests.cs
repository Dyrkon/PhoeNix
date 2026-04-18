using Microsoft.Playwright;

namespace PhoeNix.WebAPP.Tests.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LoginTests : PhoeNixTestBase
{
    [Test]
    public async Task LoginPage_ShouldShowSignInForm()
    {
        await Page.GotoAsync("/login");

        await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sign in" })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("User name")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Password")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_ShouldHaveRegisterLink()
    {
        await Page.GotoAsync("/login");

        await Expect(Page.GetByText("Create a new account")).ToBeVisibleAsync();
    }

    [Test]
    public async Task LoginPage_RegisterLink_ShouldNavigateToRegisterPage()
    {
        await Page.GotoAsync("/login");

        await Page.GetByText("Create a new account").ClickAsync();
        await Page.WaitForURLAsync("**/register");

        await Expect(Page).ToHaveURLAsync(new Regex(".*/register$"));
    }

    [Test]
    public async Task LoginPage_InvalidCredentials_ShouldShowErrorAlert()
    {
        await Page.GotoAsync("/login");

        await Page.GetByLabel("User name").FillAsync("invaliduser");
        await Page.GetByLabel("Password").FillAsync("wrongpassword");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in" }).ClickAsync();

        await Expect(Page.Locator(".mud-alert")).ToBeVisibleAsync();
    }
}