using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using LibVLCSharp.Shared;
using PZPK.Desktop.Common;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TextMateSharp.Model;

namespace PZPK.Desktop.VideoPreview;

internal class MediaModel
{
    public BehaviorSubject<string> Name { get; init; } = new("");
    public BehaviorSubject<int> Current { get; init; } = new(0);
    public BehaviorSubject<int> Total { get; init; } = new(0);

    public BehaviorSubject<bool> Playing { get; init; } = new(false);
    public BehaviorSubject<double> Position { get; init; } = new(0);
    public BehaviorSubject<double> Duration { get; init; } = new(0);
    public BehaviorSubject<double> Volumn { get; init; } = new(100);
    public BehaviorSubject<bool> SliderHolding { get; init; } = new(false);

    public IObservable<string> DurationText { get; init; }
    public IObservable<string> PositionText { get; init; }

    public Subject<RoutedEventArgs> PlayEvent { get; init; } = new();
    public Subject<RoutedEventArgs> PauseEvent { get; init; } = new();
    public Subject<RoutedEventArgs> StopEvent { get; init; } = new();
    public Subject<RangeBaseValueChangedEventArgs> SeekEvent { get; init; } = new();
    public Subject<RoutedEventArgs> NextEvent { get; init; } = new();
    public Subject<RoutedEventArgs> PrevEvent { get; init; } = new();

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


