using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PZPK.Desktop.Localization;

public record LanguageItem(string Name, string Value)
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = Name;

    [JsonPropertyName("value")]
    public string Value { get; set; } = Value;
}
public class LanguageJson
{
    [JsonPropertyName("languages")]
    public List<LanguageItem> Languages { get; set; } = new();

    [JsonPropertyName("fields")]
    public List<string> Fields { get; set; } = new();

    [JsonPropertyName("default")]
    public string DefaultLanguage { get; set; } = string.Empty;
}

public class Translate
{
    const string DefaultLanguage = "en";
    public string Current { get; private set; } = DefaultLanguage;
    private readonly List<LanguageItem> _languages = new();
    public IList<LanguageItem> Languages => _languages;
    public event Action? LanguageChanged;

    public void Initialize()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string langFilePath = Path.Join(rootPath, "Localization", "languages.json");

        var langText = File.ReadAllText(langFilePath);
        var langJson = JsonSerializer.Deserialize<LanguageJson>(langText);

        if (langJson == null)
        {
            throw new Exception();
        }

        _languages.Clear();
        _languages.AddRange(langJson.Languages);

        string userSetLang = GetSetting();
        LoadLanguage(userSetLang);
        Current = userSetLang;
    }
    public void ChangeLanguage(LanguageItem lang)
    {
        if (Current == lang.Value) return;

        Current = lang.Value;
        LoadLanguage(lang.Value);
        Settings.Set(lang);

        LanguageChanged?.Invoke();
    }
    private string GetSetting()
    {
        string? userSetCurrent = Settings.Get(SettingsField.Language);
        if (String.IsNullOrEmpty(userSetCurrent))
        {
            userSetCurrent = DefaultLanguage;
            Settings.Set(SettingsField.Language, userSetCurrent);
        }
        return userSetCurrent;
    }
    private void LoadLanguage(string lang)
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        Dictionary<string, string>? map = JsonSerializer.Deserialize<Dictionary<string, string>>(langJson);
        if (map != null) I18N.Localization.Update(map);
        else throw new Exception("Language file load error: language map is null");
    }
}
