using PZPK.Core;
using PZPK.Core.Utility;
using PZPK.Desktop.Common;
using PZPK.Desktop.Global;
using PZPK.Desktop.Main.Creator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Explorer;

public record ExtractingInfo : PZProgressState
{
    public string FilesText => $"{ProcessedFiles}/{Files}";
    public string BytesText => $"{Utility.ComputeFileSize(ProcessedBytes)}/{Utility.ComputeFileSize(Bytes)}";
    public double Percent => Bytes == 0 ? 0 : (double)ProcessedBytes / Bytes * 100;

    public CancellationTokenSource? CancelSource { get; set; }
}

public class ExplorerModel : PageModelBase
{
    private static ExplorerModel? _instance;
    public static ExplorerModel Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public PZPKPackage? Package => PZPKPackage.Current;

    public event Action? OnPackageOpened;
    public event Action? OnPackageClosed;
    public event Action<bool>? OnExtractingChanged;
    public event Action? OnExtractingProgressed;

    public bool Extracting 
    { 
        get; 
        set
        {
            if (field == value) return;

            field = value;
            OnExtractingChanged?.Invoke(field);
        }
    } = false;
    public ExtractingInfo ExtractingState = new();

    private ExplorerModel() { }
    public void OpenPackage(string path, string password)
    {
        if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(password))
        {
            try
            {
                PZPKPackage.Open(path, password);
                OnPackageOpened?.Invoke();
            }
            catch (Exception ex)
            {
                Toast.Error(ex.Message);
                Logger.Instance.Log(ex.Message);
            }
        }
    }
    public void ClosePackage()
    {
        Package?.Close();
        OnPackageClosed?.Invoke();
    }

    public async void ExtractFile(PZFile file, string dest)
    {
        if (Package == null)
        {
            Toast.Error("No package opened.");
            return;
        }

        if (File.Exists(dest))
        {
            Toast.Error("File already exists.");
            return;
        }

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractingState.CopyFrom(e);
            OnExtractingProgressed?.Invoke();
        };

        using FileStream fs = File.Create(dest);
        ExtractingState.Reset();
        ExtractingState.CancelSource = new CancellationTokenSource();

        try
        {
            // For large files, show extracting UI directly.
            if (file.Size > Constants.Sizes.t_256MB)
            {
                Extracting = true;
            }

            var count = await Package.Package.ExtractFileAsync(file, fs, progress, ExtractingState.CancelSource.Token);
            Toast.Success($"file extracted successfully.");
        }
        catch (OperationCanceledException)
        {
            Toast.Warning("Extraction cancelled.");
        }
        catch (Exception ex)
        {
            Toast.Error($"Extraction failed: {ex.Message}");
            Logger.Instance.Log(ex.ToString());
        }
        finally
        {
            Extracting = false;
            ExtractingState.CancelSource = null;
        }        
    }
    public async void ExtractFolder(PZFolder folder, string dest)
    {
        if (Package == null)
        {
            Toast.Error("No package opened.");
            return;
        }
        DirectoryInfo destDir = new(dest);

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractingState.CopyFrom(e);
            OnExtractingProgressed?.Invoke();
        };

        ExtractingState.Reset();
        ExtractingState.CancelSource = new CancellationTokenSource();

        try
        {
            Extracting = true;
            var count = await Package.Package.ExtractFolderAsync(folder, destDir, progress, ExtractingState.CancelSource.Token);
            Toast.Success($"Total {count} files extracted successfully.");
        }
        catch (OperationCanceledException)
        {
            Toast.Warning("Extraction cancelled.");
        }
        catch (Exception ex)
        {
            Toast.Error($"Extraction failed: {ex.Message}");
            Logger.Instance.Log(ex.ToString());
        }
        finally
        {
            Extracting = false;
            ExtractingState.CancelSource = null;
        }
    }
    public async void ExtractBatch(List<IPZItem> items, string dest)
    {
        if (Package == null)
        {
            Toast.Error("No package opened.");
            return;
        }
        DirectoryInfo destDir = new(dest);

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractingState.CopyFrom(e);
            OnExtractingProgressed?.Invoke();
        };

        ExtractingState.Reset();
        ExtractingState.CancelSource = new CancellationTokenSource();

        try
        {
            Extracting = true;
            var count = await Package.Package.ExtractBatchAsync(items, destDir, progress, ExtractingState.CancelSource.Token);
            Toast.Success($"Total {count} files extracted successfully.");
        }
        catch (OperationCanceledException)
        {
            Toast.Warning("Extraction cancelled.");
        }
        catch (Exception ex)
        {
            Toast.Error($"Extraction failed: {ex.Message}");
            Logger.Instance.Log(ex.ToString());
        }
        finally
        {
            Extracting = false;
            ExtractingState.CancelSource = null;
        }
    }
    public void CancelExtracting()
    {
        ExtractingState.CancelSource?.Cancel();
    }

    public async void DebugExtract()
    {
        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractingState.CopyFrom(e);
            OnExtractingProgressed?.Invoke();
        };

        ExtractingState.Reset();
        ExtractingState.CancelSource = new CancellationTokenSource();
        Extracting = true;
        await Task.Run(() =>
        {
            var state = new PZProgressState();
            var total = 36 * 400000;

            state.Bytes = total;
            state.Files = 36;
            state.CurrentBytes = 400000;

            for (int i = 0; i < 36; i++)
            {
                state.ProcessedFiles = i;
                for (int j = 0; j < 400000; j++)
                {
                    Thread.Sleep(166);
                    j += 2000;

                    state.ProcessedBytes = i * 400000 + j;
                    state.CurrentProcessedBytes = j;
                    progress.Report(state);

                    if (ExtractingState.CancelSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

        }, ExtractingState.CancelSource.Token);

        Toast.Success("Debug extraction completed.");
        Extracting = false;
    }
}
