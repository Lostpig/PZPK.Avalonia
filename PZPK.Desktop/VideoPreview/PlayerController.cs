using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Material.Icons;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.VideoPreview;

internal class PlayerController : PZComponentBase
{
    private readonly MediaModel _model;

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
                            .IsVisible(_model.Playing.Select(x => !x))
                            .RxClick(_model.PlayEvent),
                        IconButton(MaterialIconKind.Pause)
                            .IsVisible(_model.Playing)
                            .RxClick(_model.PauseEvent),
                        IconButton(MaterialIconKind.Stop)
                            .RxClick(_model.StopEvent),
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
            .IsSnapToTickEnabled(false)
            .Maximum(_model.Duration)
            .Minimum(0)
            .Value(_model.Position)
            .SmallChange(3)
            .LargeChange(3)
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
}
