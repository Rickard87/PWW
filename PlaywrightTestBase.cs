using Microsoft.Playwright;

namespace UiTests.Base;

public abstract class PlaywrightTestBase
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected readonly TestTarget Target;

    protected PlaywrightTestBase(TestTarget target)
    {
        Target = target;
    }

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = Target.Browser.ToLower() switch
        {
            "chromium" => await Playwright.Chromium.LaunchAsync(
                new() { Headless = Target.Headless }
            ),
            "firefox" => await Playwright.Firefox.LaunchAsync(new() { Headless = Target.Headless }),
            "webkit" => await Playwright.Webkit.LaunchAsync(new() { Headless = Target.Headless }),
            _ => throw new ArgumentException($"Unknown browser: {Target.Browser}"),
        };
    }

    [SetUp]
    public async Task Setup()
    {
        if (!string.IsNullOrEmpty(Target.Device))
        {
            var device = Playwright.Devices[Target.Device];
            Context = await Browser.NewContextAsync(device);
        }
        else
        {
            Context = await Browser.NewContextAsync();
        }
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
