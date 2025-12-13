using PZ.RxAvalonia.Reactive;
using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Core.Utility;
using PZPK.Desktop.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace PZPK.Desktop.Main.Creator;

using static Utility;

public class CreateProperties
{
    public BehaviorSubject<string> Name { get; init; } = new("");
    public BehaviorSubject<string> Description { get; init; } = new("");
    public ReactiveList<string> Tags { get; init; } = [];
    public BehaviorSubject<string> Password { get; init; } = new("");
    public BehaviorSubject<int> BlockSize { get; init; } = new(Constants.Sizes.t_1MB);
    public bool Check()
    {
        if (string.IsNullOrWhiteSpace(Name.Value))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(Password.Value))
        {
            return false;
        }

        if (BlockSize.Value % 1024 != 0)
        {
            return false;
        }

        return true;
    }
    public void Reset()
    {
        Name.OnNext("");
        Description.OnNext("");
        Tags.Clear();
        Password.OnNext("");
        BlockSize.OnNext(Constants.Sizes.t_1MB);
    }
}
public class ResizerProperties
{
    public BehaviorSubject<bool> Enabled = new(true);
    public BehaviorSubject<ImageResizerFormat> Format = new(ImageResizerFormat.Jpeg);
    public BehaviorSubject<int> MaxSize = new(2160);
    public BehaviorSubject<int> Quality = new(75);
    public BehaviorSubject<bool> Lossless = new(false);

    public void Reset()
    {
        Enabled.OnNext(true);
        Format.OnNext(ImageResizerFormat.Jpeg);
        MaxSize.OnNext(2160);
        Quality.OnNext(75); 
        Lossless.OnNext(false);
    }
}

public class PackingInfomation
{
    public BehaviorSubject<string> SavePath { get; init; } = new("");
    public BehaviorSubject<bool> Running { get; init; } = new(false);
    public Subject<PZProgressState> Progress { get; init; } = new();

    public IObservable<double> Percent { get; init; }
    public IObservable<string> FilesText { get; init; }
    public IObservable<string> BytesText { get; init; }

    public PackingInfomation()
    {
        Percent = Progress.Where(p => p.Bytes > 0).Select(p => ComputePercent(p.ProcessedBytes, p.Bytes));
        FilesText = Progress.Select(p => $"{p.ProcessedFiles} / {p.Files}");
        BytesText = Progress.Select(p => $"{ComputeFileSize(p.ProcessedBytes)} / {ComputeFileSize(p.Bytes)}");
    }

    public void Reset()
    {
        SavePath.OnNext("");
        Running.OnNext(false);
    }
}
public record CompleteInfomation(string PackagePath, long Size, int Count, TimeSpan UsedTime)
{
    public double Speed
    {
        get
        {
            if (UsedTime.TotalSeconds == 0) return 0;
            return Size / UsedTime.TotalSeconds;
        }
    }
}

public class CreatorModel : PageModelBase
{
    private static CreatorModel? _instance;
    public static CreatorModel Instance 
    {
        get {
            _instance ??= new CreatorModel();
            return _instance;
        }
    }

    public IndexCreator Index { get; init; }
    public CreateProperties Properties { get; init; }
    public ResizerProperties Resizer { get; init; }
    public PackingInfomation PackingInfo { get; init; }
    private CancellationTokenSource? CancelSource { get; set; }

    /// <summary>
    /// 1:Index 2:Properties 3:Packing 4:Complete
    /// </summary>
    private readonly BehaviorSubject<int> _step = new(1);
    public IObservable<int> Step { get; init; }
    public Subject<CompleteInfomation> Completed { get; init; }
    private CreatorModel()
    {
        Index = new IndexCreator();
        Properties = new CreateProperties();
        Resizer = new ResizerProperties();
        PackingInfo = new PackingInfomation();

        Completed = new();
        Step = _step.AsObservable();
    }

    public void NextStep()
    {
        if (_step.Value == 1 && !Index.IsEmpty)
        {
            _step.OnNext(2);
        }
        else if (_step.Value == 2 && Properties.Check())
        {
            _step.OnNext(3);
            PackingInfo.Progress.OnNext(new(Index.FilesCount, Index.SumFilesSize()));
        }
        else if (_step.Value == 3)
        {
            _step.OnNext(4);
        }
    }
    public void PreviousStep()
    {
        if (_step.Value > 1)
        {
            _step.Reducer(s => s - 1);
        }
    }
    public void Reset()
    {
        Index.Reset();
        Properties.Reset();
        PackingInfo.Reset();
        _step.OnNext(1);
    }

    private ImageResizer? GetImageResizer()
    {
        if (Properties.EnableImageResizing.Value)
        {
            return ImageResizer.CreateResizer(Properties.ResizeOptions.Value);
        }
        else return null;
    }
    public async void Start()
    {
        if (string.IsNullOrWhiteSpace(PackingInfo.SavePath.Value))
        {
            return;
        }

        if (!Index.IsEmpty && Properties.Check())
        {
            PackingOptions options = new(
                Properties.Password.Value,
                Properties.BlockSize.Value,
                PZType.Package,
                Properties.Name.Value,
                Properties.Description.Value,
                [.. Properties.Tags]);
            IImageResizer? imageResizer = GetImageResizer();
            PZProgress<PZProgressState> progress = new();
            var progressSubscription = Observable.FromEventPattern<PZProgressState>(
                    h => progress.ProgressChanged += h,
                    h => progress.ProgressChanged -= h
                ).Select(p => p.EventArgs).Subscribe(PackingInfo.Progress.OnNext);

            DateTime startTime = DateTime.Now;
            CancelSource = new CancellationTokenSource();
            PackingInfo.Running.OnNext(true);
            long total = await Packer.PackAsync(savePath, Index, options, progress, imageResizer, CancelSource.Token);
            PackingInfo.Running.OnNext(false);

            if (CancelSource.IsCancellationRequested)
            {
                PackingInfo.Progress.OnNext(new(Index.FilesCount, Index.SumFilesSize()));
                Toast.Warning("Info", "Packing canceled!");
            }
            else
            {
                Toast.Success("Success", "Packing complete!");
                NextStep();
                Completed.OnNext(new(savePath, total, Index.FilesCount, DateTime.Now - startTime));
            }

            CancelSource = null;
        }
    }
    public void Cancel()
    {
        if (CancelSource != null && !CancelSource.IsCancellationRequested)
        {
            CancelSource.Cancel();
        }
    }
}
