using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LibVLCSharp.Shared;
using PZPK.Core;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reactive.Linq;

namespace PZPK.Desktop.VideoPreview;

internal class VideoPreviewWindow : Window
{
    private static LibVLC VLC => VideoPreviewManager.VLC;
    private readonly VlcVideoView _videoView;
    private readonly MediaPlayer _player;
    private Window _controllerWin;
    private readonly MediaModel _mediaModel;
    private List<IDisposable> _subscriptions = [];
    public VideoPreviewWindow()
    {
        _mediaModel = new();
        _videoView = new VlcVideoView();
        _player = new(VLC);

        Content = new Panel()
            .Children(
                _videoView
            );

        InitializePlayer();
        InitializeEvent();
        InitializeControllerWindow();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    #region overlay controller window
    [MemberNotNull(nameof(_controllerWin))]
    private void InitializeControllerWindow()
    {
        _controllerWin = new Window
        {
            SystemDecorations = SystemDecorations.None,
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
            Background = Brushes.Transparent,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            ShowInTaskbar = false,
            ZIndex = int.MaxValue,
            Opacity = 1.0,
            DataContext = DataContext,
            Content = new PlayerController(_mediaModel)
        };

        Opened += ShowControllerWindow;
        LayoutUpdated += LayoutUpdate;
        PositionChanged += LayoutUpdate;
        _controllerWin.PointerEntered += FloatingContentOnPointerEvent;
        _controllerWin.PointerExited += FloatingContentOnPointerEvent;
        _controllerWin.PointerPressed += FloatingContentOnPointerEvent;
        _controllerWin.PointerReleased += FloatingContentOnPointerEvent;
    }
    private void ShowControllerWindow(object? sender, EventArgs e)
    {
        _controllerWin.Show(this);
    }
    private void UpdateOverlayPosition()
    {
        if (!IsVisible)
        {
            return;
        }

        bool forceSetWidth = false, forceSetHeight = false;
        var topLeft = new Point();
        var child = _controllerWin.Presenter?.Child;

        if (child?.IsArrangeValid == true)
        {
            switch (child.HorizontalAlignment)
            {
                case Avalonia.Layout.HorizontalAlignment.Right:
                    topLeft = topLeft.WithX(Bounds.Width - _controllerWin.Bounds.Width);
                    break;
                case Avalonia.Layout.HorizontalAlignment.Center:
                    topLeft = topLeft.WithX((Bounds.Width - _controllerWin.Bounds.Width) / 2);
                    break;
                case Avalonia.Layout.HorizontalAlignment.Stretch:
                    forceSetWidth = true;
                    break;
                case Avalonia.Layout.HorizontalAlignment.Left:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (child.VerticalAlignment)
            {
                case Avalonia.Layout.VerticalAlignment.Bottom:
                    topLeft = topLeft.WithY(Bounds.Height - _controllerWin.Bounds.Height);
                    break;
                case Avalonia.Layout.VerticalAlignment.Center:
                    topLeft = topLeft.WithY((Bounds.Height - _controllerWin.Bounds.Height) / 2);
                    break;
                case Avalonia.Layout.VerticalAlignment.Stretch:
                    forceSetHeight = true;
                    break;
                case Avalonia.Layout.VerticalAlignment.Top:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (forceSetWidth && forceSetHeight)
            _controllerWin.SizeToContent = SizeToContent.Manual;
        else if (forceSetHeight)
            _controllerWin.SizeToContent = SizeToContent.Width;
        else if (forceSetWidth)
            _controllerWin.SizeToContent = SizeToContent.Height;
        else
            _controllerWin.SizeToContent = SizeToContent.Manual;

        _controllerWin.Width = forceSetWidth ? Bounds.Width : double.NaN;
        _controllerWin.Height = forceSetHeight ? Bounds.Height : double.NaN;

        _controllerWin.MaxWidth = Bounds.Width;
        _controllerWin.MaxHeight = Bounds.Height;

        var newPosition = this.PointToScreen(topLeft);

        if (newPosition != _controllerWin.Position)
        {
            _controllerWin.Position = newPosition;
        }

        if (_controllerWin.Content is Visual content && VisualRoot is Visual root && this is Visual videoView && child != null)
        {
            content.Clip = GetVisibleRegionAsGeometry(root, videoView, child.Margin);
        }
    }
    private static RectangleGeometry? GetVisibleRegionAsGeometry(Visual parent, Visual child, Thickness childMargin)
    {
        var childPosition = child.TranslatePoint(new Point(0, 0), parent);

        if (!childPosition.HasValue) return null;

        var topDistance = childPosition.Value.Y + childMargin.Top;
        var leftDistance = childPosition.Value.X + childMargin.Left;
        var bottomDistance = parent.Bounds.Height - (childPosition.Value.Y + child.Bounds.Height + childMargin.Bottom);
        var rightDistance = parent.Bounds.Width - (childPosition.Value.X + child.Bounds.Width + childMargin.Right);

        var region = new Rect(0, 0, child.Bounds.Width, child.Bounds.Height);

        if (topDistance < 0)
        {
            region = new Rect(region.X, region.Y - topDistance, region.Width, region.Height + topDistance);
        }

        if (leftDistance < 0)
        {
            region = new Rect(region.X - leftDistance, region.Y, region.Width + leftDistance, region.Height);
        }

        if (rightDistance < 0)
        {
            region = region.WithWidth(region.Width + rightDistance);
        }

        if (bottomDistance < 0)
        {
            region = region.WithHeight(region.Height + bottomDistance);
        }

        return new RectangleGeometry(region);
    }
    private void LayoutUpdate(object? sender, EventArgs e) => UpdateOverlayPosition();
    private void FloatingContentOnPointerEvent(object? sender, PointerEventArgs e)
    {
        RaiseEvent(e);
    }
    private void DisposeControllerWindow()
    {
        LayoutUpdated -= LayoutUpdate;
        PositionChanged -= LayoutUpdate;

        _controllerWin.PointerEntered -= FloatingContentOnPointerEvent;
        _controllerWin.PointerExited -= FloatingContentOnPointerEvent;
        _controllerWin.PointerPressed -= FloatingContentOnPointerEvent;
        _controllerWin.PointerReleased -= FloatingContentOnPointerEvent;

        _controllerWin.Close();
    }
    #endregion

    #region videoview player
    public void InitializeEvent()
    {
        _subscriptions.AddRange(
                _mediaModel.PlayEvent.Subscribe(_ => {
                        if (_player.State == VLCState.Paused) _player.Pause();
                        else _player.Play();
                    }),
                _mediaModel.PauseEvent.Subscribe(_ => _player.Pause()),
                _mediaModel.StopEvent.Subscribe(_ => _player.Stop()),
                _mediaModel.SeekEvent.Throttle(TimeSpan.FromSeconds(0.33))
                    .Subscribe(e => {
                        e.Handled = true;

                        var diff = e.NewValue - (_player.Time / 1000);
                        if (diff > 3 || diff < -3) 
                        {
                            Debug.WriteLine($"Player seek from {_player.Time / 1000} to {e.NewValue}");
                            _player.SeekTo(TimeSpan.FromSeconds(e.NewValue));
                        }
                    }),
                _mediaModel.Volumn.Subscribe(n => _player.Volume = (int)n),
                _mediaModel.PrevEvent.Subscribe(_ => ChangeFile(-1)),
                _mediaModel.NextEvent.Subscribe(_ => ChangeFile(1))
            );
    }
    public void InitializePlayer()
    {
        _videoView.MediaPlayer = _player;

        _player.Opening += Player_LengthChanged;
        _player.MediaChanged += Player_LengthChanged;
        _player.LengthChanged += Player_LengthChanged;

        _player.Playing += Player_StateChanged;
        _player.Stopped += Player_StateChanged;
        _player.Paused += Player_StateChanged;
        _player.TimeChanged += Player_TimeChanged;
    }

    public void ChangeFile(int moved)
    {
        if (_mediaModel.Total.Value <= 1) return;
        var newValue = (_mediaModel.Current.Value - 1) + moved;

        if (newValue < 0) return;
        if (newValue >= _mediaModel.Total.Value) return;

        var newFile = Files[newValue];
        OpenFile(newFile);
        _player.Play();
    }

    private void Player_LengthChanged(object? sender, EventArgs e)
    {
        _mediaModel.Duration.OnNext(_player.Length / 1000.0);
    }
    private void Player_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        if (_mediaModel.SliderHolding.Value) return;
        _mediaModel.Position.OnNext(e.Time / 1000.0);
    }
    private void Player_StateChanged(object? sender, EventArgs e)
    {
        _mediaModel.Playing.OnNext(_player.State == VLCState.Playing);

        var isStoped = _player.State != VLCState.Playing && _player.State != VLCState.Paused;
        if (isStoped)
        {
            _mediaModel.Position.OnNext(0);
            if (_player.Time == _player.Length)
            {
                ChangeFile(1);
            }
        }
    }
    #endregion

    private List<PZFile> Files { get; set; } = [];
    public void OpenFile(PZFile file, List<PZFile> files)
    {
        if (!PackageManager.HasOpened) return;

        Files = files;
        _mediaModel.Total.OnNext(Files.Count);

        OpenFile(file);
    }
    private void OpenFile(PZFile file)
    {
        if (!PackageManager.HasOpened) return;

        var stream = PackageManager.Current.GetFileStream(file);

        OpenStream(stream);

        var current = Files.IndexOf(file);
        _mediaModel.Current.OnNext(current + 1);

    }
    public void OpenStream(Stream stream)
    {
        _player.Media?.Dispose();

        var input = new StreamMediaInput(stream);
        _player.Media = new Media(VLC, input);
    }
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        base.OnClosing(e);

        Hide();
    }
    protected override void OnClosed(EventArgs e)
    {
        DisposeControllerWindow();

        _subscriptions.ForEach(s => s.Dispose());

        _player.Opening -= Player_LengthChanged;
        _player.MediaChanged -= Player_LengthChanged;
        _player.LengthChanged -= Player_LengthChanged;

        _player.Playing -= Player_StateChanged;
        _player.Stopped -= Player_StateChanged;
        _player.Paused -= Player_StateChanged;
        _player.TimeChanged -= Player_TimeChanged;

        base.OnClosed(e);
    }
}
