using Avalonia;
using System.IO;
using System.Threading.Tasks;

namespace PZPK.Desktop.VideoPreview;

internal class VideoPreviewWindow: Window
{
    private VlcMeidaElement _element;
    private MediaModel _mediaModel;
    public VideoPreviewWindow()
    {
        _mediaModel = new();
        _element = new VlcMeidaElement(_mediaModel);

        Content = new Panel()
            .Children(
                _element
            );

        this.AttachDevTools();
    }

    public void OpenStream(Stream stream)
    {
        _element.OpenStream(stream);
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        base.OnClosing(e);

        Hide();
    }
    public Task ShowPreview(Window owner)
    {
        _element.ReShow();
        return ShowDialog(owner);
    }
}
