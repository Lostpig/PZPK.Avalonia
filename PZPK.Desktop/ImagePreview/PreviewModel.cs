using PZPK.Core;
using PZPK.Desktop.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.ImagePreview;

public enum LockMode
{
    None,
    Scale,
    FitHeight,
    FitWidth
}

public class PreviewModel
{
    private static PreviewModel? _instance;
    public static PreviewModel Instance
    {
        get
        {
            _instance ??= new PreviewModel();
            return _instance;
        }
    }

    public ConditionBehaviorSubject<double> Scale { get; init; }
    public IObservable<string> ScalePercent { get; init; }
    public BehaviorSubject<object> Lock { get; init; }
    public BehaviorSubject<string> FileName { get; init; }
    public ConditionBehaviorSubject<int> Current { get; init; }
    public BehaviorSubject<int> Total { get; init; }
    public IObservable<string> IndexText { get; init; }

    public BehaviorSubject<Avalonia.Size> ContainerSize { get; init; }
    public BehaviorSubject<Avalonia.PixelSize> Size { get; init; }
    public IObservable<string> SizeText { get; init; }

    public BehaviorSubject<long> FileSize { get; init; }
    public IObservable<string> FileSizeText { get; init; }

    public BehaviorSubject<bool> FullScreen { get; init; }

    private PreviewModel() 
    {
        Scale = new(1, s => s >= 0.1 && s <= 5);
        ScalePercent = Scale.Select(x => (x * 100).ToString("f1"));
        Lock = new(LockMode.None);
        FileName = new(string.Empty);

        Total = new(1);
        Current = new(0, i => i >= 0 && i < Total.Value);
        IndexText = Observable.When(Current.And(Total).Then((c, t) => $"{c}/{t}"));

        ContainerSize = new(new Avalonia.Size(1,1));
        Size = new(Avalonia.PixelSize.Empty);
        SizeText = Size.Select(s => $"{s.Width} x {s.Height}");

        FileSize = new(0);
        FileSizeText = FileSize.Select(Utility.ComputeFileSize);

        FullScreen = new(false);
    }

    public void FitToHeight()
    {
        double viewerHeight = ContainerSize.Value.Height;
        double imageHeight = Size.Value.Height;
        Scale.OnNext(viewerHeight / imageHeight);
    }
    public void FitToWidth()
    {
        double viewerWidth = ContainerSize.Value.Width;
        double imageWidth = Size.Value.Width;
        Scale.OnNext(viewerWidth / imageWidth);
    }
    public static string LockModeName(LockMode lockMode)
    {
        return lockMode switch
        {
            LockMode.None => "None",
            LockMode.Scale => "Lock Scale",
            LockMode.FitHeight => "LockFitToHeight",
            LockMode.FitWidth => "LockFitToWidth",
            _ => "None"
        };
    }
}
