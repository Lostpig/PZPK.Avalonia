using PZPK.Core;
using PZPK.Core.Extract;
using PZPK.Core.Utility;
using PZPK.Desktop.Common;
using PZPK.Desktop.Previews;
using PZPK.Desktop.Previews.ImagePreview;
using PZPK.Desktop.Previews.VideoPreview;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Explorer;

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

    public BehaviorSubject<Package?> Package = new(null);

    public BehaviorSubject<bool> IsExtracting = new(false);
    public Subject<PZProgressState> ExtractProgress = new();
    private CancellationTokenSource? ExtractCTS;

    public BehaviorSubject<string> FilePath = new("");
    public BehaviorSubject<string> Password = new("");

    public void OpenPackage()
    {
        var path = FilePath.Value;
        var password = Password.Value;
        if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(password))
        {
            try
            {
                Package.OnNext(PackageManager.Open(path, password));
            }
            catch (Exception ex)
            {
                OnErrorCatch(ex);
            }
        }
    }
    public void ClosePackage()
    {
        PackageManager.Close();
        Package.OnNext(null);

        FilePath.OnNext("");
        Password.OnNext("");
    }

    public async void ExtractFile(PZFile file, string dest)
    {
        PackageManager.Check();
        var package = PackageManager.Current;

        if (File.Exists(dest))
        {
            Toast.Error(LOC.Error.FileExistsed);
            return;
        }

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractProgress.OnNext(e);
        };

        using FileStream fs = File.Create(dest);
        ExtractCTS = new CancellationTokenSource();

        try
        {
            // For large files, show extracting UI directly.
            if (file.Size > Constants.Sizes.t_16MB)
            {
                IsExtracting.OnNext(true);
            }

            var count = await package.ExtractFileAsync(file, fs, progress, ExtractCTS.Token);
            Toast.Success(string.Format(LOC.Message.ExtractedSuccess, 1));
        }
        catch (OperationCanceledException)
        {
            Toast.Warning(LOC.Message.ExtractCancelWarning);
        }
        catch (Exception ex)
        {
            Toast.Error(string.Format(LOC.Error.ExtractFailed, ex.Message));
            Logger.Instance.Error(ex);
        }
        finally
        {
            IsExtracting.OnNext(false);
            ExtractCTS = null;
        }        
    }
    public async void ExtractFolder(PZFolder folder, string dest)
    {
        PackageManager.Check();

        DirectoryInfo destDir = new(dest);

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractProgress.OnNext(e);
        };

        ExtractCTS = new CancellationTokenSource();

        try
        {
            IsExtracting.OnNext(true);
            var count = await PackageManager.Current.ExtractFolderAsync(folder, destDir, progress, ExtractCTS.Token);
            Toast.Success(string.Format(LOC.Message.ExtractedSuccess, count));
        }
        catch (OperationCanceledException)
        {
            Toast.Warning(LOC.Message.ExtractCancelWarning);
        }
        catch (Exception ex)
        {
            Toast.Error(string.Format(LOC.Error.ExtractFailed, ex.Message));
            Logger.Instance.Error(ex);
        }
        finally
        {
            IsExtracting.OnNext(false);
            ExtractCTS = null;
        }
    }
    public async void ExtractBatch(List<IPZItem> items, string dest)
    {
        PackageManager.Check();

        DirectoryInfo destDir = new(dest);

        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractProgress.OnNext(e);
        };

        ExtractCTS = new CancellationTokenSource();

        try
        {
            IsExtracting.OnNext(true);
            var count = await PackageManager.Current.ExtractBatchAsync(items, destDir, progress, ExtractCTS.Token);
            Toast.Success(string.Format(LOC.Message.ExtractedSuccess, count));
        }
        catch (OperationCanceledException)
        {
            Toast.Warning(LOC.Message.ExtractCancelWarning);
        }
        catch (Exception ex)
        {
            Toast.Error(string.Format(LOC.Error.ExtractFailed, ex.Message));
            Logger.Instance.Error(ex);
        }
        finally
        {
            IsExtracting.OnNext(false);
            ExtractCTS = null;
        }
    }
    public void CancelExtracting()
    {
        ExtractCTS?.Cancel();
    }

    public async void DebugExtract()
    {
        PZProgress<PZProgressState> progress = new();
        progress.ProgressChanged += (s, e) =>
        {
            ExtractProgress.OnNext(e);
        };

        ExtractCTS = new CancellationTokenSource();
        IsExtracting.OnNext(true);
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

                    if (ExtractCTS.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

        }, ExtractCTS.Token);

        Toast.Success("Debug extraction completed.");
        IsExtracting.OnNext(false);
    }
}
