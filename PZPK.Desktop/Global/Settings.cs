using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PZPK.Desktop.Global;

public class JsonSettings
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    [JsonPropertyName("base_theme")]
    public string BaseTheme { get; set; } = "dark";

    [JsonPropertyName("color_theme")]
    public string ColorTheme { get; set; } = "blue";
}

public class Settings
{
    public static JsonSettings Default { get; set; } = new JsonSettings();

    public static void Load()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "settings.json");

        var jsonText = File.ReadAllText(filePath);
        var settings = JsonSerializer.Deserialize<JsonSettings>(jsonText);

        Default = settings ?? new JsonSettings();
    }

    public static void Save()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "settings.json");

        var jsonText = JsonSerializer.Serialize<JsonSettings>(Default);
        File.WriteAllText(filePath, jsonText);
    }
}

