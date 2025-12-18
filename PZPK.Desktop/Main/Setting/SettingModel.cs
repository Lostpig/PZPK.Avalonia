using Avalonia.Styling;
using PZPK.Desktop.Localization;
using SukiUI;
using SukiUI.Models;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Main.Setting;

public class SettingModel : PageModelBase
{
    private static SettingModel? _instance;
    public static SettingModel Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public SukiTheme Theme { get; init; }
    public IReadOnlyList<LanguageItem> Languages { get; init; }
    public IList<ThemeVariant> BaseThemes { get; init; }
    public IList<SukiColorTheme> ColorThemes { get; init; }

    public BehaviorSubject<SukiColorTheme> ColorTheme { get; init; }
    public BehaviorSubject<ThemeVariant> BaseTheme { get; init; }
    public BehaviorSubject<LanguageItem> ActiveLanguage { get; init; }

    private SettingModel()
    {
        Theme = SukiTheme.GetInstance();
        BaseThemes = [ThemeVariant.Light, ThemeVariant.Dark];
        ColorThemes = [.. Theme.ColorThemes];
        Languages = Translate.Languages;

        ColorTheme = new(Settings.ColorTheme);
        BaseTheme = new(Settings.BaseTheme);
        ActiveLanguage = new(Settings.Language);

        ColorTheme.Subscribe(ChangeColorTheme);
        BaseTheme.Subscribe(ChangeBaseTheme);
        ActiveLanguage.Subscribe(ChangeLanguge);
    }

    public void ChangeColorTheme(SukiColorTheme theme)
    {
        Settings.ColorTheme = theme;
    }
    public void ChangeBaseTheme(ThemeVariant theme)
    {
        Settings.BaseTheme = theme;
    }
    public static async void ChangeLanguge(LanguageItem language)
    {
        Settings.Language = language;
    }
}
