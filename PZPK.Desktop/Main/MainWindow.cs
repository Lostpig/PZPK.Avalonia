using Avalonia.Platform;
using PZPK.Desktop.Global;
using PZPK.Desktop.Localization;
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

        Sidemenu = new SukiSideMenu()
        {
            IsSearchEnabled = false
        };
        Initialize();
    }

    private SukiSideMenu Sidemenu;
    private void Initialize()
    {
        foreach (var p in Routes.Pages)
        {
            var item = new SukiSideMenuItem()
            {
                Icon = MaterialIcon(p.Icon),
                Header = p.PageName,
                PageContent = p
            };

            Sidemenu.Items.Add(item);
        }

        Content = Sidemenu;
    }

    public void BindingTranslate(Translate translate)
    {
        translate.LanguageChanged += UpdateState;
    }
    protected void UpdateState()
    {
        foreach (var item in Sidemenu.Items)
        {
            if (item is SukiSideMenuItem ssmi)
            {
                ssmi.Header = ((PageRecord)ssmi.PageContent).PageName;
            }
        }
    }

    public void DebugReRender()
    {
        Sidemenu.Items.Clear();
        Sidemenu.Content = null;
        PageLocator.Instance.Reset();

        Sidemenu = new SukiSideMenu()
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

            Sidemenu.Items.Add(item);
        }

        Content = Sidemenu;
    }
}
