using Avalonia.Controls;
using Avalonia.Platform;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;

namespace PZPK.Desktop.Main;

using static Common.ControlHelpers;

public class MainWindow : SukiWindow
{
    public ISukiToastManager ToastManager { get; init; }
    public ISukiDialogManager DialogManager { get; init; }

    public MainWindow() : base()
    {
        var sideMenu = new SukiSideMenu()
        {
            IsSearchEnabled = false
        };

        foreach (var p in Routes.Pages)
        {
            var item = new SukiSideMenuItem()
            {
                Icon = MaterialIcon(p.Icon),
                Header = p.PageName,
                PageContent = p
            };

            sideMenu.Items.Add(item);
        }

        Content = sideMenu;

        ToastManager = new SukiToastManager();
        DialogManager = new SukiDialogManager();
        var ToastHost = new SukiToastHost { Manager = ToastManager };
        var DialogHost = new SukiDialogHost() { Manager = DialogManager };

        Hosts.Add(ToastHost);
        Hosts.Add(DialogHost);

        var icon = AssetLoader.Open(new Uri($"avares://PZPK.DesktopMvu/avalonia-logo.ico"));
        Icon = new WindowIcon(icon);

        Title = "PZPK Desktop Mvu";
        Width = 1280;
        Height = 720;
    }
}
