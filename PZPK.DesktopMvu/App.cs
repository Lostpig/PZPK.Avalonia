using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Platform;
using PZPK.Desktop.Common;
using PZPK.Desktop.Modules.ImagePreview;
using SukiUI.Controls;
using System;

namespace PZPK.Desktop;

internal class App
{
    static private App? _instance;
    static public App Instance {
        get {

            _instance ??= new();
            return _instance;
        }
    }

    public Window? MainWindow { get; private set; }
    public Routes Routes { get; private set; }
    public SukiHelpers Suki { get; private set; }

    private App()
    {
        Routes = new();
        Suki = new();
    }
    public Window StartMainWindow()
    {
        var icon = AssetLoader.Open(new Uri($"avares://PZPK.DesktopMvu/avalonia-logo.ico"));

        MainWindow = new SukiWindow()
        {
            Title = "PZPK Desktop Mvu",
            Width = 1280,
            Height = 720,
            Icon = new WindowIcon(icon),
            Content = new MainContent(Routes)
        };

        MainWindow.OnClosed((_) =>
        {
            ImagePreviewManager.CloseActiveWindow();
            Environment.Exit(0);
        });

        return MainWindow;
    }
}
