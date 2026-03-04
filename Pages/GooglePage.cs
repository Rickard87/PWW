using Microsoft.Playwright;

namespace PWW;

public class GooglePage
{
    private readonly IPage _page;

    public GooglePage(IPage page)
    {
        _page = page;
    }

    private ILocator DenyCookies =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Avvisa alla" });

    public async Task NavigateToStartPage()
    {
        await _page.GotoAsync("https://www.google.se");
        await DenyCookies.ClickAsync();
    }
}
