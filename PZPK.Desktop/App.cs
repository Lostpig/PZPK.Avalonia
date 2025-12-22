using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using PZPK.Desktop.Localization;
using PZPK.Desktop.Main;
using PZPK.Desktop.VideoPreview;

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

    private App()
    {
        Suki = new();
        try
        {
            Translate.Initialize();
            Settings.Initialize();
        }
        catch
        {
            // Do nothing
        }

        MainWindow = new MainWindow();
        MainWindow.OnClosed((_) =>
        {
            ImagePreviewManager.ClosePreviewWindow();
            VideoPreviewManager.ClosePreviewWindow();
            Environment.Exit(0);
        });
    }
}
