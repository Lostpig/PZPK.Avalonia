using Avalonia.Styling;
using PZPK.Desktop.Localization;
using SukiUI;
using SukiUI.Models;
using System.Linq;

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
    public bool IsLightTheme
    {
        get => Theme.ActiveBaseTheme == ThemeVariant.Light;
        set 
        {
            var bt = value == true ? ThemeVariant.Light : ThemeVariant.Dark;
            Theme.ChangeBaseTheme(bt);
            Settings.Set(bt);
        }
    }
    public SukiColorTheme? ColorTheme => Theme.ActiveColorTheme;
    public IList<LanguageItem> Languages { get; init; }
    public LanguageItem? ActiveLanguage 
    { 
        get; 
        set
        {
            field = value;
            ChangeLanguge(value);
        }
    }

    public SettingModel()
    {
        Theme = SukiTheme.GetInstance();
        Languages = App.Instance.Translate.Languages;

        var current = Languages.FirstOrDefault(l => l.Value == App.Instance.Translate.Current);
        ActiveLanguage = current;
    }

    public void ChangeColorTheme(SukiColorTheme theme)
    {
        Theme.ChangeColorTheme(theme);
        Settings.Set(theme);
    }
    public async void ChangeLanguge(LanguageItem? language)
    {
        if (language is null) return;

        var tl = App.Instance.Translate;
        if (tl.Current == language.Value) return;

        tl.ChangeLanguage(language);
    }
}
