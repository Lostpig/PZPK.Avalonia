using Avalonia.Styling;
using PZPK.Desktop.Localization;
using SukiUI;
using SukiUI.Models;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PZPK.Desktop.Global;

public static class SettingsField
{
    public const string Language = "language";
    public const string BaseTheme = "baseTheme";
    public const string ColorTheme = "colorTheme";
}

public static class Settings
{
    private static bool _initialized = false;
    private static readonly Dictionary<string, string> data = [];
    private static void Set(string key, string value)
    {
        if (data.TryGetValue(key, out string? oldValue) && oldValue == value)
        {
            return;
        }
        else
        {
            data[key] = value;
            Save();
        }
    }
    private static string Get(string key, string defaultValue)
    {
        if (data.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public static event Action? ThemeChanged;

    public static ThemeVariant BaseTheme
    {
        get 
        {
            var value = Get(SettingsField.BaseTheme, "light");
            return value switch
            {
                "default" => ThemeVariant.Default,
                "dark" => ThemeVariant.Dark,
                "light" => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
        }
        set
        {
            string v = "";
            if (value == ThemeVariant.Default) v = "default";
            else if (value == ThemeVariant.Light) v = "light";
            else if (value == ThemeVariant.Dark) v = "dark";

            Set(SettingsField.BaseTheme, v);
            SukiTheme.GetInstance().ChangeBaseTheme(value);
            ThemeChanged?.Invoke();
        }
    }
    public static SukiColorTheme ColorTheme
    {
        get
        {
            var value = Get(SettingsField.ColorTheme, "blue");
            var theme = SukiTheme.GetInstance();
            foreach (var c in theme.ColorThemes)
            {
                if (c.DisplayName == value)
                {
                    return c;
                }
            }

            return theme.ColorThemes[0];
        }
        set
        {
            string v = value.DisplayName;

            Set(SettingsField.ColorTheme, v);
            SukiTheme.GetInstance().ChangeColorTheme(value);
            ThemeChanged?.Invoke();
        }
    }
    public static LanguageItem Language
    {
        get
        {
            var v = Get(SettingsField.ColorTheme, Translate.Default);
            var item = Translate.Languages.FirstOrDefault(l => l.Value == v) ?? Translate.Languages[0];
            return item;
        }
        set
        {
            Set(SettingsField.Language, value.Value);
            Translate.ChangeLanguage(value);
        }
    }

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Join(rootPath, "settings.json");

            if (!File.Exists(filePath))
            {
                SetDefault();
                return;
            }

            var jsonText = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

            if (settings != null)
            {
                data.Clear();
                foreach (var s in settings)
                {
                    data.Add(s.Key, s.Value);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorProxy.CatchException(ex);
            SetDefault();
        }
        finally
        {
            _initialized = true;
            SukiTheme.GetInstance().ChangeBaseTheme(BaseTheme);
            SukiTheme.GetInstance().ChangeColorTheme(ColorTheme);
            Translate.ChangeLanguage(Language);
        }
    }
    private static void SetDefault()
    {
        data.Clear();
        data.Add(SettingsField.BaseTheme, "light");
        data.Add(SettingsField.ColorTheme, "blue");
        data.Add(SettingsField.Language, Translate.Default);

        Save();
    }

    public static void Save()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "settings.json");

        var jsonText = JsonSerializer.Serialize(data);
        File.WriteAllText(filePath, jsonText);
    }
}

