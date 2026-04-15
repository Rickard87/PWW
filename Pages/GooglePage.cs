using Microsoft.Playwright;

namespace PWW;

public class GooglePage
{
    private readonly IPage _page;
    protected TestConfig Config { get; }

    public GooglePage(IPage page)
    {
        _page = page;
        Config = ConfigLoader.Cached;
    }

    private ILocator DenyCookies =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Avvisa alla" });

    public async Task NavigateToStartPage()
    {
        if (!string.IsNullOrEmpty(Config.BaseUrl))
            await _page.GotoAsync(Config.BaseUrl);
        if (await DenyCookies.IsVisibleAsync())
            await DenyCookies.ClickAsync();
    }
}
