using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Core.Utility;
using PZPK.Desktop.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PZPK.Desktop.Main.Creator;

public class CreateProperties
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string Password { get; set; } = "";
    public int BlockSize { get; set; } = Constants.Sizes.t_1MB;
    public bool EnableImageResizing { get; set; } = true;
    public ImageResizerOptions ImageOptions { get; set; } = new();
    public bool Check()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            return false;
        }

        if (BlockSize % 1024 != 0)
        {
            return false;
        }

        return true;
    }
    public void Reset()
    {
        Name = "";
        Description = "";
        Tags = [];
        Password = "";
        BlockSize = Constants.Sizes.t_1MB;

        EnableImageResizing = true;
        ImageOptions.MaxSize = 2160;
        ImageOptions.Quality = 75;
        ImageOptions.Format = ImageResizerFormat.Jpeg;
        ImageOptions.Lossless = false;
    }
}
public class PackingInfomation
{
    public string SavePath { get; set; } = "";

    public bool Running { get; private set; } = false;
    public int Files { get; private set; }  = 0;
    public int TotalFiles { get; private set; } = 0;
    public long Bytes { get; private set; } = 0;
    public long TotalBytes { get; private set; } = 0;

    public double Percent => TotalBytes == 0 ? 0 : ((double)Bytes / TotalBytes) * 100.0;
    public string FilesText => $"{Files} / {TotalFiles}";
    public string BytesText => $"{Utility.ComputeFileSize(Bytes)} / {Utility.ComputeFileSize(TotalBytes)}";

    public void Update(int files, int totalFiles, long bytes, long totalBytes)
    {
        Files = files;
        TotalFiles = totalFiles;
        Bytes = bytes;
        TotalBytes = totalBytes;
    }
    public void Reset()
    {
        Running = false;
        Files = 0;
        TotalFiles = 0;
        Bytes = 0;
        TotalBytes = 0;
    }

    public void Start()
    {
        Running = true;
    }
    public void Complete() 
    { 
        Running = false; 
    }
}
public class CompleteInfomation
{
    public string PackagePath { get; set; } = "";
    public long PackageSize { get; set; } = 0;
    public int FilesCount { get; set; } = 0;
    public TimeSpan UsedTime { get; set; } = TimeSpan.Zero;
    public double Speed
    {
        get
        {
            if (UsedTime.TotalSeconds == 0) return 0;
            return PackageSize / UsedTime.TotalSeconds;
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
    public PackingInfomation PackingInfo { get; init; }
    private CancellationTokenSource? CancelSource { get; set; }
    public CompleteInfomation CompleteInfo { get; init; }

    /// <summary>
    /// 1:Index 2:Properties 3:Packing 4:Complete
    /// </summary>
    public int Step { 
        get; 
        set {
            if (field == value) return;
            field = value;
            OnStepChanged?.Invoke();
        } 
    } = 1; 
    public event Action? OnStepChanged;
    public event Action? OnPackingProgressed;

    private CreatorModel()
    {
        Index = new IndexCreator();
        Properties = new CreateProperties();
        PackingInfo = new PackingInfomation();
        CompleteInfo = new CompleteInfomation();
    }

    public void NextStep()
    {
        if (Step == 1)
        {
            if (!Index.IsEmpty) Step++;
        }
        else if (Step == 2)
        {
            if (Properties.Check())
            {
                PackingInfo.Update(0, Index.FilesCount, 0, Index.SumFilesSize());
                Step++;
            }
        }
        else if (Step == 3)
        {
            Step++;
        }
    }
    public void PreviousStep()
    {
        if (Step > 1)
        {
            Step--;
        }
    }
    public void Reset()
    {
        Index.Reset();
        Properties.Reset();
        PackingInfo.Reset();
        Step = 1;
    }

    private ImageResizer? GetImageResizer()
    {
        if (Properties.EnableImageResizing)
        {
            return ImageResizer.CreateResizer(Properties.ImageOptions);
        }
        else return null;
    }
    public async void Start(string savePath)
    {
        if (!Index.IsEmpty && Properties.Check())
        {
            PackingOptions options = new(
                Properties.Password,
                Properties.BlockSize,
                PZType.Package,
                Properties.Name,
                Properties.Description,
                [.. Properties.Tags]);
            IImageResizer? imageResizer = GetImageResizer();
            PZProgress<PZProgressState> progress = new();
            progress.ProgressChanged += (s, e) =>
            {
                PackingInfo.Update(e.ProcessedFiles, e.Files, e.ProcessedBytes, e.Bytes);
                OnPackingProgressed?.Invoke();
            };

            DateTime startTime = DateTime.Now;

            CancelSource = new CancellationTokenSource();
            PackingInfo.Start();
            long total = await Packer.PackAsync(savePath, Index, options, progress, imageResizer, CancelSource.Token);
            PackingInfo.Complete();

            if (CancelSource.IsCancellationRequested)
            {
                PackingInfo.Update(0, Index.FilesCount, 0, Index.SumFilesSize());
                Toast.Warning("Info", "Packing canceled!");
                OnPackingProgressed?.Invoke();
            }
            else
            {
                CompleteInfo.PackagePath = savePath;
                CompleteInfo.PackageSize = total;
                CompleteInfo.UsedTime = DateTime.Now - startTime;
                CompleteInfo.FilesCount = Index.FilesCount;

                Toast.Success("Success", "Packing complete!");
                NextStep();
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
