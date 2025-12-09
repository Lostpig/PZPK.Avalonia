using System;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using PZPK.Desktop.Localization;
using PZPK.Desktop.Main;

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

    public MainWindow MainWindow { get; init; }
    public SukiHelpers Suki { get; init; }
    public Translate Translate { get; init; }

    private App()
    {
        Suki = new();
        Translate = new();
        try
        {
            Translate.Initialize();
        }
        catch
        {
            // Do nothing
        }

        MainWindow = new MainWindow();
        MainWindow.OnClosed((_) =>
        {
            ImagePreviewManager.CloseActiveWindow();
            Environment.Exit(0);
        });
    }
}
