using Microsoft.Playwright;

namespace UiTests.Base;

public abstract class PlaywrightTestBase
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected string BrowserName { get; }
    protected TestConfig Config { get; }

    protected PlaywrightTestBase(string browserName)
    {
        Config = ConfigLoader.Load();
        BrowserName = browserName;
    }

    protected virtual bool Headless => Config.Headless;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = BrowserName.ToLower() switch
        {
            "chromium" => await Playwright.Chromium.LaunchAsync(new() { Headless = Headless }),
            "firefox" => await Playwright.Firefox.LaunchAsync(new() { Headless = Headless }),
            "webkit" => await Playwright.Webkit.LaunchAsync(new() { Headless = Headless }),
            _ => throw new ArgumentException("Unknown browser"),
        };
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
