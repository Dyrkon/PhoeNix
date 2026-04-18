using Microsoft.Playwright;

namespace PhoeNix.WebAPP.Tests.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegistrationTests : PhoeNixTestBase
{
    [Test]
    public async Task RegisterPage_ShouldShowRegisterForm()
    {
        await Page.GotoAsync("/register");

        await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Register" })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("User name")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Password")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Confirm password")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Register" }))
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task RegisterPage_ShouldHaveLoginLink()
    {
        await Page.GotoAsync("/register");

        await Expect(Page.GetByText("Already have an account")).ToBeVisibleAsync();
    }

    [Test]
    public async Task RegisterPage_LoginLink_ShouldNavigateToLoginPage()
    {
        await Page.GotoAsync("/register");

        await Page.GetByText("Already have an account").ClickAsync();
        await Page.WaitForURLAsync("**/login");

        await Expect(Page).ToHaveURLAsync(new Regex(".*/login$"));
    }
}