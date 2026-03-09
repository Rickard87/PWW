using Microsoft.Playwright;
using UiTests.Base;

namespace PWW;

[TestFixtureSource(nameof(Targets))]
public class GoogleTests : PlaywrightTestBase
{
    public static IEnumerable<TestTarget> Targets => ConfigLoader.Load().Targets;

    public GoogleTests(TestTarget target)
        : base(target) { }

    [SetUp]
    public async Task TestSetup()
    {
        // Frivillig TestSetup - Undvik namnet "Setup" som finns i abstractet PlaywrightTestBase
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
