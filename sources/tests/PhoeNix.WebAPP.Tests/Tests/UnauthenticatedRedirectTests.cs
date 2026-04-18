namespace PhoeNix.WebAPP.Tests.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class UnauthenticatedRedirectTests : PhoeNixTestBase
{
    [Test]
    public async Task Root_WhenUnauthenticated_ShouldRedirectToLogin()
    {
        await Page.GotoAsync("/");
        await Page.WaitForURLAsync("**/login");

        await Expect(Page).ToHaveURLAsync(new Regex(".*/login$"));
    }

    [TestCase("/configurations")]
    [TestCase("/setup")]
    [TestCase("/templates")]
    [TestCase("/settings")]
    public async Task ProtectedPage_WhenUnauthenticated_ShouldRedirectToLogin(string path)
    {
        await Page.GotoAsync(path);
        await Page.WaitForURLAsync("**/login");

        await Expect(Page).ToHaveURLAsync(new Regex(".*/login$"));
    }
}
