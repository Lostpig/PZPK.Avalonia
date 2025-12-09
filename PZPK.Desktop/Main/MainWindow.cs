using Avalonia.Platform;
using PZPK.Desktop.Global;
using SukiUI.Controls;
using System.Reflection;

namespace PZPK.Desktop.Main;

using static Common.ControlHelpers;

public class MainWindow : PZWindowBase
{
    public MainWindow() : base()
    {
        var icon = AssetLoader.Open(new Uri($"avares://PZPK.Desktop/avalonia-logo.ico"));
        Icon = new WindowIcon(icon);

        Title = "PZPK Desktop V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
        Title += " (Debug)";
#endif

        Width = 1280;
        Height = 720;

        Render();
    }

    public void Render()
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
    }
}
