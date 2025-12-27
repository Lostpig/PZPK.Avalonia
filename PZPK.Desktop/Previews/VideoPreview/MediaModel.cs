using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using LibVLCSharp.Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Previews.VideoPreview;

internal enum PlayAction
{
    Play,
    Pause,
    Stop
}
internal class MediaModel
{
    public BehaviorSubject<string> Name { get; init; } = new("");
    public BehaviorSubject<int> Current { get; init; } = new(0);
    public BehaviorSubject<int> Total { get; init; } = new(0);

    public BehaviorSubject<VLCState> State { get; init; } = new(VLCState.NothingSpecial);
    public BehaviorSubject<double> Position { get; init; } = new(0);
    public BehaviorSubject<double> Duration { get; init; } = new(1);
    public BehaviorSubject<double> Volumn { get; init; } = new(100);
    public BehaviorSubject<bool> SliderHolding { get; init; } = new(false);

    public IObservable<string> DurationText { get; init; }
    public IObservable<string> PositionText { get; init; }

    public Subject<PlayAction> PlayEvent { get; init; } = new();
    public Subject<RangeBaseValueChangedEventArgs> SeekEvent { get; init; } = new();
    public Subject<RoutedEventArgs> NextEvent { get; init; } = new();
    public Subject<RoutedEventArgs> PrevEvent { get; init; } = new();
    public Subject<double> ForwardChange { get; init; } = new();

    public MediaModel()
    {
        DurationText = Duration.Select(FormatTime);
        PositionText = Position.Select(FormatTime);
    }

    private static string FormatTime(double t)
    {
        if (t <= 0) return "00:00";

        var span = TimeSpan.FromSeconds(t);
        if (span.TotalHours > 1) return $"{span.Days * 24 + span.Hours}:{span.Minutes:d2}:{span.Seconds:d2}";
        else return $"{span.Minutes:d2}:{span.Seconds:d2}";
    }
}


