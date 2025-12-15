using Avalonia.Styling;
using PZPK.Desktop.Localization;
using SukiUI;
using SukiUI.Models;
using System.Linq;
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
    public IList<LanguageItem> Languages { get; init; }
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
        Languages = App.Instance.Translate.Languages;

        ColorTheme = new(Theme.ActiveColorTheme!);
        BaseTheme = new(Theme.ActiveBaseTheme);
        ActiveLanguage = new(Languages[0]);

        ColorTheme.Subscribe(ChangeColorTheme);
        BaseTheme.Subscribe(ChangeBaseTheme);
        ActiveLanguage.Subscribe(ChangeLanguge);
    }

    public void ChangeColorTheme(SukiColorTheme theme)
    {
        Theme.ChangeColorTheme(theme);
        Settings.Set(theme);
    }
    public void ChangeBaseTheme(ThemeVariant theme)
    {
        Theme.ChangeBaseTheme(theme);
        Settings.Set(theme);
    }
    public static async void ChangeLanguge(LanguageItem? language)
    {
        if (language is null) return;

        var tl = App.Instance.Translate;
        if (tl.Current == language.Value) return;

        tl.ChangeLanguage(language);
    }
}
