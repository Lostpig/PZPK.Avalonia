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
    public int BlockSize { get; set; } = Constants.Sizes.t_4KB;
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
}
public class PackingInfomation
{
    public bool Running { get; private set; } = false;
    public int Files { get; private set; }  = 0;
    public int TotalFiles { get; private set; } = 0;
    public long Bytes { get; private set; } = 0;
    public long TotalBytes { get; private set; } = 0;

    public double Percent => TotalBytes == 0 ? 0 : (double)Bytes / TotalBytes;
    public string FilesText => $"{Files} / {TotalFiles}";
    public string BytesText => $"{Utility.ComputeFileSize(Bytes)} / {Utility.ComputeFileSize(TotalBytes)}";

    public void Update(int files, int totalFiles, long bytes, long totalBytes)
    {
        Files = files;
        TotalFiles = totalFiles;
        Bytes = bytes;
        TotalBytes = totalBytes;
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

public class CreatorModel
{
    public IndexCreator Index;
    public CreateProperties Properties;
    public PackingInfomation PackingInfo;
    private CancellationTokenSource? cancelSource;

    //1:Index 2:Properties 3:Packing 4:Complete
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

    public CreatorModel()
    {
        Index = new IndexCreator();
        Properties = new CreateProperties();
        PackingInfo = new PackingInfomation();
    }

    public void NextStep()
    {
        if (Step == 1)
        {
            if (!Index.IsEmpty) Step++;
        }
        else if (Step == 2)
        {
            if (Properties.Check()) Step++;
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
        Index = new IndexCreator();
        Properties = new CreateProperties();
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
            Progress<PZProgressState> progress = new();
            progress.ProgressChanged += (s, e) =>
            {
                PackingInfo.Update(e.ProcessedFiles, e.Files, e.ProcessedBytes, e.Bytes);
                OnPackingProgressed?.Invoke();
            };

            cancelSource = new CancellationTokenSource();
            PackingInfo.Start();
            long total = await Packer.PackAsync(savePath, Index, options, progress, imageResizer, cancelSource.Token);
            PackingInfo.Complete();

            if (cancelSource.IsCancellationRequested)
            {
                PackingInfo.Update(0, Index.FilesCount, 0, Index.SumFilesSize());
                Toast.ShowToast("Info", "Packing canceled!");
                OnPackingProgressed?.Invoke();
            }
            else
            {
                NextStep();
            }

            cancelSource = null;
        }
    }
    public void Cancel()
    {
        if (cancelSource != null && !cancelSource.IsCancellationRequested)
        {
            cancelSource.Cancel();
        }
    }
}
