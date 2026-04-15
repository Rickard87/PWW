using System.Text.Json;

public class TestTarget
{
    public string Name { get; set; } = "";
    public string Browser { get; set; } = "";
    public string? Device { get; set; }
    public bool Headless { get; set; }
}

public class TestConfig
{
    public string BaseUrl { get; set; } = "";
    public List<TestTarget> Targets { get; set; } = new();
}

public static class ConfigLoader
{
    private static readonly string[] AllowedBrowsers = { "chromium", "firefox", "webkit" };

    public static TestConfig Cached { get; } = Load();

    public static TestConfig Load(string path = "playwrightconfig.json")
    {
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<TestConfig>(json) ?? new TestConfig();
        Validate(config);
        return config;
    }

    private static void Validate(TestConfig config)
    {
        foreach (var t in config.Targets)
        {
            if (!AllowedBrowsers.Contains(t.Browser?.ToLowerInvariant()))
                throw new InvalidOperationException(
                    $"Target '{t.Name}': unknown Browser '{t.Browser}'. Use one of: {string.Join(", ", AllowedBrowsers)}."
                );
        }
    }
}
