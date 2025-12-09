using PZPK.Core.Crypto;
using PZPK.Core.Utility;

namespace PZPK.Core.Extract;

public class Package : IDisposable
{
    private readonly FileStream _stream;
    private readonly IPZCrypto _crypto;

    public PZHeader Header { get; private set; }
    public PZDetail Detail { get; private set; }

    public PackageIndex Index { get; private set; }

    internal Package(FileStream stream, string password)
    {
        _stream = stream;

        Header = HeaderExtractor.ExtractHeader(_stream);
        CheckPackage();

        byte[] key = PZCrypto.CreateKey(password);
        _crypto = PZCrypto.Create(Header.Version, key, Header.BlockSize);

        HeaderExtractor.CheckPassword(_crypto, Header);

        Detail = DetailExtractor.ExtractDetail(Header, _crypto, _stream);
        Index = IndexExtractor.ExtractIndex(Header, _crypto, _stream);
    }

    private void CheckPackage()
    {
        if (Header.Type != PZType.Package)
        {
            throw new Exceptions.FileTypeMismatchException(Constants.GetTypeIDText(Header.Type), "Package");
        }
    }

    public byte[] ExtractData(long offset, long length)
    {
        Span<byte> input = new byte[length];
        Span<byte> output = new byte[length];

        _stream.Position = offset;
        _stream.ReadExactly(input);
        var decryptedLength = _crypto.Decrypt(input, output);

        return output[..decryptedLength].ToArray();
    }
    public byte[] ExtractFile(PZFile file)
    {
        Span<byte> decrypted = new byte[file.OriginSize];

        var length = _crypto.DecryptFile(_stream, file, decrypted);
        return decrypted[..length].ToArray();
    }

    public long ExtractFileToPath(PZFile file, string destination, Action<long, long>? progress = default)
    {
        var dir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using FileStream fs = File.Create(destination);
        return _crypto.DecryptStream(_stream, file.Offset, file.Size, fs, progress);
    }
    public long ExtractFileStream(PZFile file, Stream destination, Action<long, long>? progress = default)
    {
        return _crypto.DecryptStream(_stream, file.Offset, file.Size, destination, progress);
    }
    public Task<long> ExtractFileAsync(PZFile file, Stream destination, IProgress<PZProgressState>? progress = default, CancellationToken? cancelToken = null)
    {
        PZProgressState state = new();
        state.Reset();
        state.Files = 1;
        state.Bytes = file.Size;

        void currentProgress(long readed, long total)
        {
            cancelToken?.ThrowIfCancellationRequested();

            state.CurrentBytes = total;
            state.CurrentProcessedBytes = readed;
            progress?.Report(state);
        }
        return Task.Run(() => ExtractFileStream(file, destination, currentProgress), cancelToken ?? CancellationToken.None);
    }

    public int ExtractFolder(PZFolder folder, DirectoryInfo destination, IProgress<PZProgressState>? progress = default, CancellationToken? cancelToken = null)
    {
        if (!destination.Exists)
        {
            destination.Create();
        }
        var files = Index.GetFiles(folder, true);

        PZProgressState state = new();
        state.Reset();
        state.Files = files.Count;
        state.Bytes = files.Sum(f => f.Size);
        void currentProgress(long readed, long total)
        {
            cancelToken?.ThrowIfCancellationRequested();

            state.CurrentBytes = total;
            state.CurrentProcessedBytes = readed;
            progress?.Report(state);
        }

        foreach (var file in files)
        {
            string resolveFilePath = Index.GetResolvePath(file, folder);
            string dest = Path.Combine(destination.FullName, resolveFilePath);
            ExtractFileToPath(file, dest, currentProgress);

            state.ProcessedBytes += file.Size;
            state.ProcessedFiles++;
            progress?.Report(state);
        }

        return files.Count;
    }
    public Task<int> ExtractFolderAsync(PZFolder folder, DirectoryInfo destination, IProgress<PZProgressState>? progress = default, CancellationToken? cancelToken = null)
    {
        return Task.Run(() => ExtractFolder(folder, destination, progress, cancelToken), cancelToken ?? CancellationToken.None);
    }

    public int ExtractBatch(List<IPZItem> items, DirectoryInfo destination, IProgress<PZProgressState>? progress = default, CancellationToken? cancelToken = null)
    {
        if (items.Count == 0) return 0;
        if (!destination.Exists)
        {
            destination.Create();
        }

        PZFolder parentFolder = Index.GetFolder(items[0].Pid);
        List<PZFile> totalFiles = [];
        foreach (var item in items)
        {
            if (item.Pid != parentFolder.Id)
            {
                throw new ArgumentException("All items must be in the same parent folder.");
            }

            if (item is PZFile file)
            {
                totalFiles.Add(file);
            }
            else if (item is PZFolder folder)
            {
                totalFiles.AddRange(Index.GetFiles(folder, true));
            }
        }

        PZProgressState state = new();
        state.Reset();
        state.Files = totalFiles.Count;
        state.Bytes = totalFiles.Sum(f => f.Size);
        void currentProgress(long readed, long total)
        {
            cancelToken?.ThrowIfCancellationRequested();
            state.CurrentBytes = total;
            state.CurrentProcessedBytes = readed;
            progress?.Report(state);
        }

        foreach (var file in totalFiles)
        {
            string resolveFilePath = Index.GetResolvePath(file, parentFolder);
            string dest = Path.Combine(destination.FullName, resolveFilePath);
            ExtractFileToPath(file, dest, currentProgress);

            state.ProcessedBytes += file.Size;
            state.ProcessedFiles++;
            progress?.Report(state);
        }
        return state.Files;
    }
    public Task<int> ExtractBatchAsync(List<IPZItem> items, DirectoryInfo destination, IProgress<PZProgressState>? progress = default, CancellationToken? cancelToken = null)
    {
        return Task.Run(() => ExtractBatch(items, destination, progress, cancelToken), cancelToken ?? CancellationToken.None);
    }

    public PZFileStream GetFileStream(PZFile file)
    {
        return _crypto.CreatePZFileStream(_stream, file);
    }

    public void Dispose()
    {
        _stream.Close();
        _stream.Dispose();
        _crypto.Dispose();

        GC.SuppressFinalize(this);
    }
}
