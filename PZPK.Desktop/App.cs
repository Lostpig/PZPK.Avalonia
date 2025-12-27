using PZPK.Desktop.Common;
using PZPK.Desktop.Localization;
using PZPK.Desktop.Main;
using PZPK.Desktop.Previews;
using PZPK.Desktop.Previews.ImagePreview;
using PZPK.Desktop.Previews.VideoPreview;

namespace PZPK.Desktop;

public class App
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
            PreviewManager.CloseWindows();
            Environment.Exit(0);
        });
    }
}
