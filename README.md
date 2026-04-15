# PWW — Playwright Web Test Template

A `dotnet new` template that scaffolds a ready-to-run **NUnit + Playwright** web UI test project. You get a working browser-driving test suite in about a minute, with a clean layout to grow into.

PWW is **web-only** by design. API testing lives in the sibling **ATT** (API Test Template) project — don't look for API helpers here.

---

## Requirements

- **.NET 10 SDK**
- **Git**
- **VS Code** (recommended), with the **CSharpier** extension for automatic C# formatting
- A Windows / macOS / Linux machine — Playwright installs its own browser binaries on first use

---

## Install the template locally

From the root of this repository:

```bash
dotnet new install . --force
```

`--force` reinstalls over any previous version so you always pick up your latest edits to the template source.

Verify the template is registered:

```bash
dotnet new list pww
```

You should see `Playwright Web Test Template (PWW)` with short name `pww`.

---

## Scaffold a new project from the template

Pick a folder where your new test project should live, then:

```bash
dotnet new pww -n MyTests
```

This creates a `MyTests/` directory containing a full copy of the template with every `PWW` token rewritten to `MyTests` (csproj name, namespaces, etc.). Open it in VS Code and you're ready.

---

## First run

Inside your scaffolded project:

```bash
# 1. Restore + build
dotnet build

# 2. Install Playwright's browser binaries (first time only, or after a Playwright upgrade)
pwsh bin/Debug/net10.0/playwright.ps1 install

# 3. Run the tests
dotnet test
```

The example test navigates to `https://www.google.se`, dismisses the cookie banner, and passes. You will actually **see the browsers pop up** — that's intentional (see [Headless mode](#headless-mode)).

---

## Project layout

```
MyTests/
├── .template.config/
│   └── template.json          Template metadata — don't edit unless extending the template
├── Pages/
│   └── GooglePage.cs          Example Page Object
├── GoogleTests.cs             Example test fixture
├── PlaywrightTestBase.cs      Browser / context / page lifecycle
├── TestTarget.cs              Config model + ConfigLoader
├── playwrightconfig.json      BaseUrl + list of target browsers
└── MyTests.csproj
```

**Namespaces** are split by role:

| Namespace | What lives there |
|---|---|
| `MyTests.Base` | `PlaywrightTestBase` |
| `MyTests.Config` | `TestTarget`, `TestConfig`, `ConfigLoader` |
| `MyTests.Pages` | Page Object classes |
| `MyTests.Tests` | Test fixtures |

---

## Configuration: `playwrightconfig.json`

```json
{
  "BaseUrl": "https://www.google.se",
  "Targets": [
    { "Name": "Chromium Desktop", "Browser": "chromium", "Device": null, "Headless": false },
    { "Name": "WebKit iPhone 6",  "Browser": "webkit",   "Device": "iPhone 6", "Headless": false }
  ]
}
```

| Field | Meaning |
|---|---|
| `BaseUrl` | The origin every page object navigates to. Single value shared across all targets. |
| `Targets[].Name` | Human-readable label for the target. Shows up in test output. |
| `Targets[].Browser` | One of `chromium`, `firefox`, `webkit`. Validated on load — an unknown value throws immediately. |
| `Targets[].Device` | Optional. Name of a Playwright device profile (e.g. `iPhone 6`, `Pixel 5`). When set, the browser context is pre-configured with that device's viewport, user agent, and touch settings. |
| `Targets[].Headless` | `true` or `false`. Controls whether the browser is visible. |

The file is copied to the build output (`PreserveNewest`) and read once per run via `ConfigLoader.Cached`.

---

## How tests fan out across browsers / devices

Every test fixture is multiplied across the entries in `Targets`. The mechanism is:

```csharp
[TestFixtureSource(nameof(Targets))]
public class GoogleTests : PlaywrightTestBase
{
    public static IEnumerable<TestTarget> Targets => ConfigLoader.Cached.Targets;

    public GoogleTests(TestTarget target) : base(target) { }

    [Test]
    public async Task CanOpenStartPage() { ... }
}
```

With the default config, `CanOpenStartPage` runs twice — once against Chromium Desktop, once against WebKit iPhone 6. Add a third target and it runs three times. No test code changes needed.

`PlaywrightTestBase` handles the lifecycle:

| NUnit hook | What it does |
|---|---|
| `[OneTimeSetUp]` | Launches the browser chosen by `Target.Browser` with `Headless` from the config. |
| `[SetUp]` | Creates a fresh `IBrowserContext` per test (applying a device profile if `Target.Device` is set) and a new `IPage`. |
| `[TearDown]` | Closes the context. |
| `[OneTimeTearDown]` | Closes the browser. |

---

## Adding a new page object

Page objects live in `Pages/` and follow the Page Object Model. They take `IPage` in the constructor and expose locators as private properties and actions as public async methods.

```csharp
using Microsoft.Playwright;
using MyTests.Config;

namespace MyTests.Pages;

public class CheckoutPage
{
    private readonly IPage _page;
    protected TestConfig Config { get; }

    public CheckoutPage(IPage page)
    {
        _page = page;
        Config = ConfigLoader.Cached;
    }

    private ILocator PayButton => _page.GetByRole(AriaRole.Button, new() { Name = "Pay" });
    private ILocator TotalAmount => _page.GetByTestId("order-total");

    public async Task GoToCheckout() => await _page.GotoAsync($"{Config.BaseUrl}/checkout");

    public async Task PayNow() => await PayButton.ClickAsync();

    public Task<string?> GetTotalAsync() => TotalAmount.TextContentAsync();
}
```

`Config` is available on every page so you can use `Config.BaseUrl` or any other values you add to `playwrightconfig.json`.

---

## Adding a new test fixture

Mirror `GoogleTests.cs` — the static `Targets` property and the constructor forwarding are boilerplate that every fixture needs.

```csharp
using Microsoft.Playwright;
using MyTests.Base;
using MyTests.Config;
using MyTests.Pages;

namespace MyTests.Tests;

[TestFixtureSource(nameof(Targets))]
public class CheckoutTests : PlaywrightTestBase
{
    public static IEnumerable<TestTarget> Targets => ConfigLoader.Cached.Targets;

    public CheckoutTests(TestTarget target) : base(target) { }

    [Test]
    public async Task TotalDisplaysCorrectAmount()
    {
        var checkout = new CheckoutPage(Page);
        await checkout.GoToCheckout();

        var total = await checkout.GetTotalAsync();
        Assert.That(total, Does.Contain("$"));
    }
}
```

> **Naming note.** `PlaywrightTestBase` exposes a `[SetUp]` method called `InitializePage`. If you want your own per-fixture setup, name it `Setup` (or anything else) — NUnit runs every `[SetUp]` method up the inheritance chain.

---

## Writing assertions

PWW is an **NUnit** test project that happens to drive a browser via Playwright. **Use NUnit's `Assert.That(...)` API** — not `Microsoft.Playwright.Assertions.Expect(...)`. Read the values you want to check using Playwright's page APIs, then assert with NUnit.

```csharp
// ✅ Prefer this
var title = await Page.TitleAsync();
Assert.That(title, Does.Contain("Google"));

var isVisible = await myLocator.IsVisibleAsync();
Assert.That(isVisible, Is.True);

// ❌ Don't do this in PWW projects
await Expect(Page).ToHaveTitleAsync(new Regex("Google"));
```

Keeping a single assertion style across the suite makes failure messages consistent and keeps the NUnit / Playwright layers cleanly separated.

---

## Running tests

```bash
# Run everything (all fixtures × all targets)
dotnet test

# Run a specific test — the ~ match covers all target variants
dotnet test --filter "FullyQualifiedName~GoogleTests.CanOpenStartPage"

# Run only fixtures whose class name contains "Checkout"
dotnet test --filter "FullyQualifiedName~CheckoutTests"
```

---

## Headless mode

The shipped `playwrightconfig.json` sets `"Headless": false` on every target. This is deliberate — on first run you actually want to *see* the browser do something, so you know it's working.

For CI, flip them to `true`:

```json
{ "Name": "Chromium Desktop", "Browser": "chromium", "Device": null, "Headless": true }
```

You can keep separate config files (`playwrightconfig.ci.json`) and swap them in via your CI pipeline, or override `Headless` programmatically in `PlaywrightTestBase` based on an environment variable — whichever fits your workflow.

---

## Updating Playwright's browsers

When `Microsoft.Playwright` bumps its version in the csproj, reinstall the matching browser binaries:

```bash
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install
```

---

## Re-installing the template after you edit it

If you modify this source repo (not a scaffolded project — the template itself), re-register it so the next `dotnet new pww` picks up your changes:

```bash
dotnet new install . --force
```
