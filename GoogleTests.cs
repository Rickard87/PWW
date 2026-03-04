using Microsoft.Playwright;
using UiTests.Base;

namespace PWW;

public class GoogleTests : PlaywrightTestBase
{
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
