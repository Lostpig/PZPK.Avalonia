using LibVLCSharp.Shared;
using PZPK.Core;
using PZPK.Desktop.Common;
using SukiUI.Dialogs;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

    static private LibVLC? _VLCInstance;
    static public LibVLC VLC {
        get
        {
            InitializeVLC();
            return _VLCInstance!;
        }
    }

    static public async Task InitializeVLC()
    {
        if (_VLCInstance != null) return;

        var manager = App.Instance.MainWindow.Dialog.Manager;
        var builder = manager.CreateDialog()
            .WithTitle("Info")
            .WithContent("VLC Initializing");
        builder.TryShow();

        _VLCInstance = await Task.Run(() => {
            LibVLC vlc = new();
            return vlc;
        });

        manager.TryDismissDialog(builder.Dialog);
    }

    static async public void OpenVideo(PZFile file)
    {
        if (!PackageManager.HasOpened) return;
        if (!FileTypeHelper.IsVideo(file)) return;

        await InitializeVLC();

        var idx = PackageManager.Current.Index;
        var folder = idx.GetFolder(file.Pid);
        var files = idx.GetFiles(folder, false);
        var videos = files.Where(f => FileTypeHelper.IsVideo(f))
                        .ToList().Sorted(NaturalPZItemComparer.Instance);

        var win = GetWindow();
        win.OpenFile(file, files);
        await win.ShowDialog(App.Instance.MainWindow);
    }

    static public async void DevOpenVideo(string file)
    {
        await InitializeVLC();
        var fileInfo = new FileInfo(file);
        if (fileInfo.Exists)
        {
            var win = GetWindow();
            var stream = fileInfo.OpenRead();
            win.OpenStream(stream);
            await win.ShowDialog(App.Instance.MainWindow);

            stream.Dispose();
        }
    }

    static public void ClosePreviewWindow()
    {
        PreviewWindow?.Close();
        PreviewWindow = null;
    }
}
