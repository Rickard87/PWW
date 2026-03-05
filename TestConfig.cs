using System.Text.Json;

public class TestConfig
{
    public string[] Browsers { get; set; } = Array.Empty<string>();
    public bool Headless { get; set; } = false;
    public string BaseUrl { get; set; } = "";
}

public static class ConfigLoader
{
    public static TestConfig Load(string path = "playwrightconfig.json")
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestConfig>(json) ?? new TestConfig();
    }
}
