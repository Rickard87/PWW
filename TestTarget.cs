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
    public static TestConfig Load(string path = "playwrightconfig.json")
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestConfig>(json) ?? new TestConfig();
    }
}
