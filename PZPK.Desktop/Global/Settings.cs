using Avalonia.Styling;
using PZPK.Desktop.Localization;
using SukiUI;
using SukiUI.Models;
using System.IO;
using System.Text.Json;

namespace PZPK.Desktop.Global;

public static class SettingsField
{
    public const string Language = "language";
    public const string BaseTheme = "baseTheme";
    public const string ColorTheme = "colorTheme";
}

public class Settings
{
    private static readonly Dictionary<string, string> data = [];

    public static void Set(string key, string value)
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
    public static string? Get(string key)
    {
        if (data.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    public static void Set(ThemeVariant themeVariant)
    {
        string v = "";
        if (themeVariant == ThemeVariant.Default) v = "default";
        else if (themeVariant == ThemeVariant.Light) v = "light";
        else if (themeVariant == ThemeVariant.Dark) v = "dark";

        Set(SettingsField.BaseTheme, v);
    }
    public static void Set(SukiColorTheme color)
    {
        string v = color.DisplayName;
        Set(SettingsField.ColorTheme, v);
    }
    public static void Set(LanguageItem lang)
    {
        Set(SettingsField.Language, lang.Value);
    }

    public static ThemeVariant GetBaseTheme()
    {
        var value = Get(SettingsField.BaseTheme);
        return value switch
        {
            "default" => ThemeVariant.Default,
            "dark" => ThemeVariant.Dark,
            "light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }
    public static SukiColorTheme GetColorTheme()
    {
        var value = Get(SettingsField.ColorTheme);
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

    public static void Load()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "settings.json");

        if (!File.Exists(filePath))
        {
            ResetDefault();
            return;
        }

        var jsonText = File.ReadAllText(filePath);
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

        if (settings == null)
        {
            ResetDefault();
            return;
        }

        data.Clear();
        foreach (var s in settings)
        {
            data.Add(s.Key, s.Value);
        }
    }
    private static void ResetDefault()
    {
        data.Clear();
        data.Add(SettingsField.BaseTheme, "default");
        data.Add(SettingsField.ColorTheme, "blue");
        data.Add(SettingsField.Language, "en");
    }

    public static void ApplySetting()
    {
        // apply themes
        var theme = SukiTheme.GetInstance();
        var baseTheme = GetBaseTheme();
        theme.ChangeBaseTheme(baseTheme);

        var colorTheme = GetColorTheme();
        theme.ChangeColorTheme(colorTheme);
    }

    public static void Save()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Join(rootPath, "settings.json");

        var jsonText = JsonSerializer.Serialize(data);
        File.WriteAllText(filePath, jsonText);
    }
}

