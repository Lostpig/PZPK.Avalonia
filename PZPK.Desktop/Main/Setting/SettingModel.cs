using Avalonia.Styling;
using PZPK.Desktop.Global;
using PZPK.Desktop.Localization;
using PZPK.Desktop.Main.Notebook;
using SukiUI;
using SukiUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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


    public JsonSettings Settings => Global.Settings.Default;
    private readonly SukiTheme _theme;

    public SettingModel()
    {
        _theme = SukiTheme.GetInstance();
        
    }

    public void ToggleBaseTheme()
    {
        _theme.SwitchBaseTheme();
    }
    public void ChangeColorTheme(SukiColorTheme theme)
    {
        _theme.ChangeColorTheme(theme);
    }
    public async void ChangeLanguge(LanguageItem language)
    {
        var tl = App.Instance.Translate;

        if (tl.Current == language.Value) return;

        var ok = await Dialog.WarningConfirm("Change language will restart appliation, sure to change?");

        if (ok)
        {
            await Task.Delay(1000);
            App.Instance.MainWindow.Render();
        }
    }
}
