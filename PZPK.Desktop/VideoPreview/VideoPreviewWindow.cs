using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System.IO;

namespace PZPK.Desktop.VideoPreview;

internal class VideoPreviewWindow: Window
{
    private LibVLC _libVlc;
    private MediaPlayer _player;
    private VideoView _videoView;
    public VideoPreviewWindow()
    {
        _libVlc = new LibVLC();
        _player = new(_libVlc);
        _videoView = new();
        _videoView.MediaPlayer = _player;
        _videoView.Content = new Button().Content("Play");

        Content = Grid("*", "*")
            .Children(
                _videoView
                    .Cell(0, 0)
            );
    }

    public void PlayStream(Stream stream)
    {
        var input = new StreamMediaInput(stream);
        _player.Media = new Media(_libVlc, input);
        _player.Play();
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        base.OnClosing(e);
        _player.Stop();
        _player.Media = null;

        Hide();
    }
}
