using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Material.Icons;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.VideoPreview;

internal class PlayerController : PZComponentBase
{
    private readonly MediaModel _model;

    protected override StyleGroup? BuildStyles()
    {
        return [
            new Style<Button>().Focusable(false),
            new Style<RepeatButton>().Focusable(false)  // TODO: this is SukiUI bug
        ];
    }

    private static TextBlock CtrlText(IObservable<string> t)
    {
        return PzText(t)
            .VerticalAlignment(VerticalAlignment.Center)
            .Foreground(() => Suki.GetSukiColor("SukiText"));
    }
    private static TextBlock CtrlText(string t)
    {
        return PzText(t)
            .VerticalAlignment(VerticalAlignment.Center)
            .Foreground(() => Suki.GetSukiColor("SukiInformationForeground"));
    }

    private Grid BuildControls()
    {
        return Grid("1*, 50, 150").Margin(20, 0)
            .Children(
                HStackPanel().Col(0)
                    .Spacing(10)
                    .Children(
                        IconButton(MaterialIconKind.Play)
                            .IsVisible(_model.State.Select(s => s != VLCState.Playing))
                            .OnClick(_ => _model.PlayEvent.OnNext(PlayAction.Play)),
                        IconButton(MaterialIconKind.Pause)
                            .IsVisible(_model.State.Select(s => s == VLCState.Playing))
                            .OnClick(_ => _model.PlayEvent.OnNext(PlayAction.Pause)),
                        IconButton(MaterialIconKind.Stop)
                            .OnClick(_ => _model.PlayEvent.OnNext(PlayAction.Stop)),
                        PzSeparatorH(),
                        IconButton(MaterialIconKind.PreviousTitle).RxClick(_model.PrevEvent),
                        CtrlText(_model.Current.Select(i => i.ToString())),
                        CtrlText("/"),
                        CtrlText(_model.Total.Select(i => i.ToString())),
                        IconButton(MaterialIconKind.NextTitle).RxClick(_model.NextEvent),
                        PzSeparatorH(),
                        CtrlText(_model.PositionText),
                        CtrlText("/"),
                        CtrlText(_model.DurationText)
                    ),
                HStackPanel().Col(2)
                    .Children(
                        MaterialIcon(MaterialIconKind.VolumeHigh),
                        new Slider().Width(120).Margin(10, 0)
                            .Cursor(new Cursor(StandardCursorType.Hand))
                            .IsSnapToTickEnabled(false)
                            .Focusable(false)
                            .Maximum(100)
                            .Minimum(0)
                            .Value(_model.Volumn)
                    )
            );
    }
    private Slider BuildProgress()
    {
        return new Slider()
            .Margin(10, 0)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .Focusable(false)
            .IsSnapToTickEnabled(false)
            .Maximum(_model.Duration)
            .Minimum(0)
            .Value(_model.Position)
            .SmallChange(5)
            .LargeChange(5)
            .OnPointerPressed(_ => _model.SliderHolding.OnNext(true))
            .OnPointerReleased(_ => _model.SliderHolding.OnNext(false))
            .RxValueChanged(_model.SeekEvent);
    }
    private Grid BuildControllerBar(Grid bar)
    {
        return bar
            .Height(110)
            .Background(Brushes.Transparent)
            .Children(
                new Canvas()
                    .IsHitTestVisible(false)
                    .Background(() => Suki.GetSukiColor("SukiDialogBackground"))
                    .Opacity(0.9),
                Grid(null, "30, 30, 40, 10").VerticalAlignment(VerticalAlignment.Bottom)
                    .Children(
                        CtrlText(_model.Name).Row(0).Margin(15, 0),
                        BuildProgress().Row(1),
                        BuildControls().Row(2)
                    )
            );
    }

    protected override Control Build()
    {
        return new DockPanel()
                .Focusable(true)
                .Background(Brushes.Transparent)
                .LastChildFill(false)
                .Children(
                    BuildControllerBar(ControllerBar).Dock(Dock.Bottom)
                );
    }

    public PlayerController(MediaModel model) : base(ViewInitializationStrategy.Lazy)
    {
        _model = model;
        ControllerBar = new Grid();

        Initialize();
        InitializeEvents();
        InitializeHotKey();
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
            Hider.Throttle(TimeSpan.FromSeconds(3))
            .Subscribe(_ => {
                Dispatcher.UIThread.Post(HideBarAnimation);
            })
        ];
    }

    private readonly Grid ControllerBar;
    private Point LastMousePosition = default;
    private bool IsBarShow = true;
    private readonly Subject<Unit> Hider = new();

    private void InitializeEvents()
    {
        var transtion = new DoubleTransition();
        transtion.Property = Visual.OpacityProperty;
        transtion.Duration = TimeSpan.FromSeconds(0.5);
        ControllerBar.Transitions = [transtion];

        PointerMoved += PlayerController_PointerMoved;
        PointerExited += PlayerController_PointerExited;
    }

    private readonly Cursor NoneCursor = new(StandardCursorType.None);
    private void HideBarAnimation()
    {
        if (!IsBarShow || ControllerBar.IsPointerOver) return;

        ControllerBar.Opacity = 0;
        Cursor = NoneCursor;
        IsBarShow = false;
    }
    private void ShowBarAnimation()
    {
        if (IsBarShow) return;

        ControllerBar.Opacity = 1;
        Cursor = Cursor.Default;
        IsBarShow = true;
    }
    private void PlayerController_PointerExited(object? sender, PointerEventArgs e)
    {
        Hider.OnNext(Unit.Default);
    }
    private void PlayerController_PointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPosition = e.GetPosition(this);
        if (Math.Abs(currentPosition.X - LastMousePosition.X) > double.Epsilon ||
            Math.Abs(currentPosition.Y - LastMousePosition.Y) > double.Epsilon)
        {
            ShowBarAnimation();
        }
            
        LastMousePosition = currentPosition;
        Hider.OnNext(Unit.Default);
    }

    #region hotkey
    private DispatcherTimer KeyDownTimer;
    private Key? CurrentDownKey;

    [MemberNotNull(nameof(KeyDownTimer))]
    private void InitializeHotKey()
    {
        KeyDown += HotKeyDown;
        KeyUp += HotKeyUp;

        KeyDownTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(333),
            IsEnabled = true,
        };
        KeyDownTimer.Tick += (e, s) =>
        {
            if (CurrentDownKey.HasValue)
            {
                MoveForward(CurrentDownKey.Value);
            }
            else
            {
                KeyDownTimer.Stop();
            }
        };
        LostFocus += (e, s) =>
        {
            if (CurrentDownKey != null)
            {
                CurrentDownKey = null;
                KeyDownTimer.Stop();
            }
        };
    }

    private void HotKeyDown(object? sender, KeyEventArgs e)
    {
        Debug.WriteLine("key down:" + e.Key);
        // e.Handled = true;
        if (e.Key == Key.Left || e.Key == Key.Right)
        {
            CurrentDownKey = e.Key;
            MoveForward(CurrentDownKey.Value);
            KeyDownTimer.Start();
        }
        else if (e.Key == Key.Space)
        {
            if (_model.State.Value == VLCState.Playing)
            {
                _model.PlayEvent.OnNext(PlayAction.Pause);
            }
            else
            {
                _model.PlayEvent.OnNext(PlayAction.Play);
            }
        }
    }
    private void HotKeyUp(object? sender, KeyEventArgs e)
    {
        // e.Handled = true;
        if (e.Key == Key.Left || e.Key == Key.Right)
        {
            if (e.Key == CurrentDownKey)
            {
                CurrentDownKey = null;
                KeyDownTimer.Stop();
            }
        }
    }
    private void MoveForward(Key key)
    {
        if (key == Key.Left)
        {
            _model.ForwardChange.OnNext(-5);
        }
        else if (key == Key.Right)
        {
            _model.ForwardChange.OnNext(5);
        }
    }
    #endregion

}
