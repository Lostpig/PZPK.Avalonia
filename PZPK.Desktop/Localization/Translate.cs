using System.Diagnostics.CodeAnalysis;
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

public static class Translate
{
    private static bool _initialized = false;
    private static List<LanguageItem> _languages = [];
    public static IReadOnlyList<LanguageItem> Languages => _languages;
    public static string Default { get; private set; } = "zh-CN";
    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string langFilePath = Path.Join(rootPath, "Localization", "languages.json");

            var langText = File.ReadAllText(langFilePath);
            var langJson = JsonSerializer.Deserialize<LanguageJson>(langText) ?? throw new Exception("languages.json deserialize failed");
            _languages.Clear();
            _languages.AddRange(langJson.Languages);

            Default = langJson.DefaultLanguage;
        }
        catch(Exception ex)
        {
            Logger.Instance.Log("Error: Translate initialize failed!");
            ErrorProxy.CatchException(ex);
        }
        finally
        {
            _initialized = true;
        }
    }

    public static LanguageItem? Current { get; private set; }
    public static event Action? LanguageChanged;

    public static void ChangeLanguage(LanguageItem lang)
    {
        if (Current?.Value == lang.Value) return;

        Current = lang;
        LoadLanguage(lang);
        LanguageChanged?.Invoke();
    }
    private static void LoadLanguage(LanguageItem lang)
    {
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang.Value}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        FieldsJson? langFields = JsonSerializer.Deserialize<FieldsJson>(langJson);
        if (langFields != null) {
            langFields.Namespaces.ForEach(ns => I18N.Updater.Update(ns.NameSpace, ns.Fields));
        }
        else throw new Exception("Language file load error: language map is null");
    }
}
