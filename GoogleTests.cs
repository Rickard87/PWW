using Microsoft.Playwright;
using PWW.Base;
using PWW.Config;
using PWW.Pages;

namespace PWW.Tests;

[TestFixtureSource(nameof(Targets))]
public class GoogleTests : PlaywrightTestBase
{
    public static IEnumerable<TestTarget> Targets => ConfigLoader.Cached.Targets;

    public GoogleTests(TestTarget target)
        : base(target) { }

    [SetUp]
    public async Task Setup()
    {
        // Frivillig per-test setup
    }

    [Test]
    public async Task CanOpenStartPage()
    {
        var google = new GooglePage(Page);
        await google.NavigateToStartPage();
        await Task.Delay(5000);
        Assert.Pass(); // Riktig Assert krävs
    }
}
