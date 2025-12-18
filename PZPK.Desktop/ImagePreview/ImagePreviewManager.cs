using PZPK.Core;
using PZPK.Desktop.Common;
using System.Linq;

namespace PZPK.Desktop.ImagePreview;

public class ImagePreviewManager
{
    static private ImagePreviewWindow? PreviewWindow;
    static private ImagePreviewWindow GetWindow()
    {
        if (PreviewWindow == null)
        {
            PreviewWindow = new()
            {
                Name = LOC.Preview.ImagePreview
            };
        }

        return PreviewWindow;
    }

    static public void OpenImage(PZFile file)
    {
        if (PackageManager.Current == null) return;
        if (!FileTypeHelper.IsPicture(file)) return;

        var idx = PackageManager.Current.Index;
        var folder = idx.GetFolder(file.Pid);
        var files = idx.GetFiles(folder, false);
        var pictures = files.Where(f => FileTypeHelper.IsPicture(f))
                        .ToList().Sorted(NaturalPZItemComparer.Instance);

        var win = GetWindow();
        win.OpenImage(file, pictures);
        if (win.IsVisible)
        {
            win.Activate();
            if (win.WindowState == WindowState.Minimized)
            {
                win.WindowState = WindowState.Normal;
            }
        }
        else
        {
            win.Show();
        }
    }

    static public void DevOpenImage(string file)
    {
        var win = GetWindow();
        win.DevOpenImage(file);
        win.Activate();
        win.Show();
    }

    static public void ClosePreviewWindow()
    {
        PreviewWindow?.Close();
    }
}
