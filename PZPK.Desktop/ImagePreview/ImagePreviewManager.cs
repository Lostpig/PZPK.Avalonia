using PZPK.Core;
using PZPK.Desktop.Common;
using System.Linq;

namespace PZPK.Desktop.ImagePreview;

public class ImagePreviewManager
{
    static private ImagePreviewWindow? ActiveWindow;
    static private ImagePreviewWindow GetActiveWindow()
    {
        if (ActiveWindow == null)
        {
            ActiveWindow = new ImagePreviewWindow();
            ActiveWindow.Name = LOC.Preview.ImagePreview;
            ActiveWindow.Closed += (_, _) => ActiveWindow = null;
            ActiveWindow.Show();
        }

        return ActiveWindow;
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

        var win = GetActiveWindow();
        win.OpenImage(file, pictures);
        win.Activate();
    }

    static public void DevOpenImage(string file)
    {
        var win = GetActiveWindow();
        win.DevOpenImage(file);
        win.Activate();
    }

    static public void CloseActiveWindow()
    {
        ActiveWindow?.Close();
    }
}
