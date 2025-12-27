using LibVLCSharp.Shared;
using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.Previews.ImagePreview;
using PZPK.Desktop.Previews.TextPreview;
using PZPK.Desktop.Previews.VideoPreview;
using SukiUI.Dialogs;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PZPK.Desktop.Previews;

internal class PreviewManager
{
    static private ImagePreviewWindow? ImageWindow;
    static private VideoPreviewWindow? VideoWindow;
    static private LibVLC? _VLCInstance;
    static private TextPreviewWindow? TextWindow;

    static public void PreviewFile(PZFile file)
    {
        var type = PZItemTypeHelper.GetItemType(file);

        if (type == PZItemType.Picture)
        {
            OpenImage(file);
            return;
        }

        if (type == PZItemType.Video)
        {
            OpenVideo(file);
            return;
        }

        if (type == PZItemType.Text)
        {
            OpenText(file);
            return;
        }
    }
    static public void CloseWindows()
    {
        CloseImageWindow();
        CloseVideoWindow();

    }

    static private ImagePreviewWindow GetImageWindow()
    {
        ImageWindow ??= new()
            {
                Title = LOC.Preview.ImagePreview
            };

        return ImageWindow;
    }
    static public void OpenImage(PZFile file)
    {
        if (PackageManager.Current == null) return;
        if (!PZItemTypeHelper.IsPicture(file)) return;

        var idx = PackageManager.Current.Index;
        var folder = idx.GetFolder(file.Pid);
        var files = idx.GetFiles(folder, false);
        var pictures = files.Where(f => PZItemTypeHelper.IsPicture(f))
                        .ToList().Sorted(NaturalPZItemComparer.Instance);

        var win = GetImageWindow();
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
        var win = GetImageWindow();
        win.DevOpenImage(file);
        win.Activate();
        win.Show();
    }
    static public void CloseImageWindow()
    {
        ImageWindow?.Close();
        ImageWindow = null;
    }

    static public async Task InitializeVLC()
    {
        if (_VLCInstance != null) return;

        var manager = App.Instance.MainWindow.Dialog.Manager;
        var builder = manager.CreateDialog()
            .WithTitle(LOC.Preview.VLCInitializing)
            .WithContent("......");
        builder.TryShow();

        _VLCInstance = await Task.Run(() => {
            LibVLC vlc = new();
            return vlc;
        });

        manager.TryDismissDialog(builder.Dialog);
    }
    static private async Task<VideoPreviewWindow> GetVideoWindow()
    {
        await InitializeVLC();
        if (VideoWindow == null)
        {
            VideoWindow = new(_VLCInstance!)
            {
                Title = LOC.Preview.VideoPreview
            };
        }

        return VideoWindow;
    }
    static async public void OpenVideo(PZFile file)
    {
        if (!PackageManager.HasOpened) return;
        if (!PZItemTypeHelper.IsVideo(file)) return;

        var idx = PackageManager.Current.Index;
        var folder = idx.GetFolder(file.Pid);
        var files = idx.GetFiles(folder, false);
        var videos = files.Where(f => PZItemTypeHelper.IsVideo(f))
                        .ToList().Sorted(NaturalPZItemComparer.Instance);

        var win = await GetVideoWindow();
        win.OpenFile(file, files);
        await win.ShowDialog(App.Instance.MainWindow);
    }
    static public async void DevOpenVideo(string file)
    {
        var fileInfo = new FileInfo(file);
        if (fileInfo.Exists)
        {
            var win = await GetVideoWindow();
            var stream = fileInfo.OpenRead();
            win.OpenStream(stream);
            await win.ShowDialog(App.Instance.MainWindow);

            stream.Dispose();
        }
    }
    static public void CloseVideoWindow()
    {
        VideoWindow?.Close();
        VideoWindow = null;
    }

    static private TextPreviewWindow GetTextWindow()
    {
        TextWindow ??= new();
        return TextWindow;
    }
    static public void OpenText(PZFile file)
    {
        if (PackageManager.Current == null) return;
        if (!PZItemTypeHelper.IsText(file)) return;

        var win = GetTextWindow();
        win.OpenFile(file);
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
    static public void CloseTextWindow()
    {
        TextWindow?.Close();
        TextWindow = null;
    }
}
