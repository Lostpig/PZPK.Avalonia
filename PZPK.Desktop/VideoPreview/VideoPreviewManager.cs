using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PZPK.Desktop.VideoPreview;

internal class VideoPreviewManager
{
    static private VideoPreviewWindow? PreviewWindow;
    static private VideoPreviewWindow GetWindow()
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

    static async public void OpenVideo(PZFile file)
    {
        if (PackageManager.Current == null) return;
        if (!FileTypeHelper.IsVideo(file)) return;

        var stream = PackageManager.Current.GetFileStream(file);

        var win = GetWindow();
        win.PlayStream(stream);
        await win.ShowDialog(App.Instance.MainWindow);

        stream.Dispose();
    }

    static public void DevOpenImage(string file)
    {

    }

    static public void ClosePreviewWindow()
    {
        PreviewWindow?.Close();
        PreviewWindow = null;
    }
}
