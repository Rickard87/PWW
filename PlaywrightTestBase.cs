using Microsoft.Playwright;
using NUnit.Framework;

namespace UiTests.Base;

public abstract class PlaywrightTestBase
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    // 🔧 Här kan du centralt styra headless
    protected virtual bool Headless => false;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = await Playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = Headless }
        );
    }

    [SetUp]
    public async Task Setup()
    {
        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await Context.CloseAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}
