using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Material.Icons;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace PZPK.Desktop.VideoPreview;

internal class MediaModel
{
    public BehaviorSubject<string> Name { get; init; } = new("");
    public BehaviorSubject<bool> Playing { get; init; } = new(false);
    public BehaviorSubject<double> Position { get; init; } = new(0);
    public BehaviorSubject<double> Duration { get; init; } = new(0);
    public BehaviorSubject<double> Volumn { get; init; } = new(0.7);

    public IObservable<string> DurationText { get; init; }
    public IObservable<string> PositionText { get; init; }
    public IObservable<double> PositionObs { get; init; }

    public Subject<RoutedEventArgs> PlayEvent { get; init; } = new();
    public Subject<RoutedEventArgs> PauseEvent { get; init; } = new();
    public Subject<RoutedEventArgs> StopEvent { get; init; } = new();
    public Subject<RangeBaseValueChangedEventArgs> SeekEvent { get; init; } = new();

    public MediaModel()
    {
        DurationText = Duration.Select(t => TimeSpan.FromSeconds(t).ToString("HH:mm:ss"));
        PositionText = Position.Select(t => TimeSpan.FromSeconds(t).ToString("HH:mm:ss"));
        PositionObs = Position.Throttle(TimeSpan.FromSeconds(0.5));
    }
}

internal class PlayerController : ComponentBase
{
    private readonly MediaModel _model;
    private Grid BuildControls()
    {
        return Grid("1*, 50, 150").Margin(20, 0)
            .Children(
                HStackPanel().Col(0)
                    .Spacing(10)
                    .Children(
                        IconButton(MaterialIconKind.Play)
                            .IsVisible(_model.Playing.Select(x => !x))
                            .RxClick(_model.PlayEvent),
                        IconButton(MaterialIconKind.Pause)
                            .IsVisible(_model.Playing)
                            .RxClick(_model.PauseEvent),
                        IconButton(MaterialIconKind.Stop)
                            .RxClick(_model.StopEvent),
                        PzSeparatorH(),
                        IconButton(MaterialIconKind.PreviousTitle),
                        IconButton(MaterialIconKind.NextTitle),
                        PzSeparatorH(),
                        PzText(_model.PositionText).VerticalAlignment(VerticalAlignment.Center),
                        PzText("/").VerticalAlignment(VerticalAlignment.Center),
                        PzText(_model.DurationText).VerticalAlignment(VerticalAlignment.Center)
                    ),
                HStackPanel().Col(2)
                    .Children(
                        MaterialIcon(MaterialIconKind.VolumeControl),
                        new Slider().Width(120).Margin(10, 0)
                            .Cursor(new Cursor(StandardCursorType.Hand))
                            .IsSnapToTickEnabled(false)
                            .Maximum(100)
                            .Minimum(0)
                    )
            ); 
    }
    protected override Control Build()
    {
        var maskColor = App.Instance.Suki.GetSukiColor("SukiDialogBackground");
        return Grid().Height(100).Background(Brushes.Transparent)
            .Children(
                new Canvas().IsHitTestVisible(false).Background(maskColor),
                Grid(null, "30, 40, 10").VerticalAlignment(VerticalAlignment.Bottom)
                    .Children(
                        new Slider()
                            .Row(0)
                            .Margin(10, 0)
                            .Cursor(new Cursor(StandardCursorType.Hand))
                            .IsSnapToTickEnabled(false)
                            .Maximum(_model.Duration)
                            .Minimum(0)
                            .Value(_model.PositionObs)
                            .SmallChange(3)
                            .LargeChange(3)
                            .RxValueChanged(_model.SeekEvent),
                        BuildControls().Row(1)
                    )
            );
    }

    public PlayerController(MediaModel model) : base(ViewInitializationStrategy.Lazy)
    {
        _model = model;
        Initialize();
    }
}

internal class VlcMeidaElement : ComponentBase
{
    private readonly MediaModel _model;
    private static LibVLC VLC => VideoPreviewManager.VLC;
    private readonly MediaPlayer _player;
    private readonly VideoView _videoView;
    private readonly DockPanel _ctrlContainer;

    protected override Control Build()
    {
        _videoView.Content = _ctrlContainer;

        return Grid()
            .Children(
                _videoView
            );
    }

    public VlcMeidaElement(MediaModel model) : base(ViewInitializationStrategy.Lazy)
    {
        _model = model;
        _videoView = new();
        _player = new(VLC);

        _ctrlContainer = new DockPanel()
                .LastChildFill(false)
                .ZIndex(99)
                .Children(
                    new PlayerController(_model).Dock(Dock.Bottom)
                );

        InitializePlayer();

        Initialize();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
                _model.PlayEvent.Subscribe(_ => {
                    if (_player.State == VLCState.Paused) _player.Pause();
                    else _player.Play();
                }),
                _model.PauseEvent.Subscribe(_ => _player.Pause()),
                _model.StopEvent.Subscribe(_ => _player.Stop()),
                _model.SeekEvent
                    .Subscribe(e => {
                        e.Handled = true;
                        
                        var diff = e.NewValue - e.OldValue;
                        if (diff > 3 || diff < -3) 
                        {
                            _player.SeekTo(TimeSpan.FromSeconds(e.NewValue));
                        }
                    })
            ];
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
    public void ReShow()
    {
        _videoView.Content = null;
        Debug.WriteLine("reshow!");
        _videoView.Content = _ctrlContainer;
    }

    private void Player_LengthChanged(object? sender, EventArgs e)
    {
        _model.Duration.OnNext(_player.Length / 1000.0);
    }
    private void Player_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        _model.Position.OnNext(e.Time / 1000.0);
    }
    private void Player_StateChanged(object? sender, EventArgs e)
    {
        _model.Playing.OnNext(_player.State == VLCState.Playing);

        var isStoped = _player.State != VLCState.Playing && _player.State != VLCState.Paused;
        if (isStoped)
        {
            _model.Position.OnNext(0);
        }
    }

    public async void OpenStream(Stream stream)
    {
        if (_player.Media != null)
        {
            _player.Media.Dispose();
        }

        var input = new StreamMediaInput(stream);
        _player.Media = new Media(VLC, input);
    }
}
