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
public record LocalizationNameSpace
{
    [JsonPropertyName("namespace")]
    public string NameSpace { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<string> Fields { get; set; } = [];
}
public record LanguageJson
{
    [JsonPropertyName("languages")]
    public List<LanguageItem> Languages { get; set; } = [];

    [JsonPropertyName("namespaces")]
    public List<LocalizationNameSpace> Namespaces { get; set; } = [];

    [JsonPropertyName("default")]
    public string DefaultLanguage { get; set; } = string.Empty;
}

public record FieldsJson
{
    [JsonPropertyName("namespaces")]
    public List<FieldsJsonNameSpace> Namespaces { get; set; } = [];
}
public record FieldsJsonNameSpace
{
    [JsonPropertyName("namespace")]
    public string NameSpace { get; set; } = string.Empty;
    [JsonPropertyName("fields")]
    public Dictionary<string, string> Fields { get; set; } = [];
}

public class Translate
{
    const string DefaultLanguage = "en";
    public string Current { get; private set; } = DefaultLanguage;
    private readonly List<LanguageItem> _languages = [];
    public IList<LanguageItem> Languages => _languages;
    public event Action? LanguageChanged;

    public void Initialize()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string langFilePath = Path.Join(rootPath, "Localization", "languages.json");

        var langText = File.ReadAllText(langFilePath);
        var langJson = JsonSerializer.Deserialize<LanguageJson>(langText) ?? throw new Exception("languages.json deserialize failed");
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
    private static string GetSetting()
    {
        string? userSetCurrent = Settings.Get(SettingsField.Language);
        if (String.IsNullOrEmpty(userSetCurrent))
        {
            userSetCurrent = DefaultLanguage;
            Settings.Set(SettingsField.Language, userSetCurrent);
        }
        return userSetCurrent;
    }
    private static void LoadLanguage(string lang)
    {
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        FieldsJson? langFields = JsonSerializer.Deserialize<FieldsJson>(langJson);
        if (langFields != null) {
            langFields.Namespaces.ForEach(ns => I18N.Updater.Update(ns.NameSpace, ns.Fields));
        }
        else throw new Exception("Language file load error: language map is null");
    }
}
