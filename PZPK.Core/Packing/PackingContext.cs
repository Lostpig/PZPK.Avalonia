using PZPK.Core.Utility;
using PZPK.Core.Exceptions;

namespace PZPK.Core.Packing;

internal class PackingContext
{
    public PackingOptions Options { get; init; }
    public IProgress<PZProgressState>? Progress { get; init; }
    public IImageResizer? ImageResizer { get; init; }
    public IndexCreator Index { get; init; }
    public PZProgressState ProgressState { get; init; }

    private readonly Dictionary<int, PZFile> _files = new();
    private readonly Dictionary<int, PZFolder> _folders = new();
    public IReadOnlyList<PZIndexFile> Files { get; private set; }

    public int DetailSize { get; private set; }
    public long DetailOffset { get; private set; }
    public int IndexSize { get; private set; }
    public long IndexOffset { get; private set; }

    public long TotalSize { get; private set; }

    public PackingContext(PackingOptions options, IndexCreator index, IImageResizer? imageResizer, IProgress<PZProgressState>? progress)
    {
        Options = options;
        ImageResizer = imageResizer;
        Progress = progress;
        Index = index;

        Files = Index.GetAllFiles();
        ProgressState = new PZProgressState();
    }

    public void Start()
    {
        Check();

        DetailSize = 0;
        DetailOffset = 0;
        IndexSize = 0;
        IndexOffset = 0;
        TotalSize = 0;
        Files = Index.GetAllFiles();

        ProgressState.Reset();
        ProgressState.Files = Index.FilesCount;
        ProgressState.Bytes = Index.SumFilesSize();
    }
    public void FileProgress(long processed, long total)
    {
        ProgressState.CurrentBytes = total;
        ProgressState.CurrentProcessedBytes = processed;
        Progress?.Report(ProgressState);
    }
    public void FileComplete(PZIndexFile file, long processedSize, long offset, string? newName, long? newSize)
    {
        newName ??= file.Name;
        var originalSize = newSize ?? file.Size;
        PZFile pzFile = new(newName, file.Id, file.Pid, offset, processedSize, originalSize);
        _files.Add(pzFile.Id, pzFile);

        EnsureFolder(pzFile.Pid);

        ProgressState.ProcessedFiles++;
        ProgressState.ProcessedBytes += file.Size;
        Progress?.Report(ProgressState);

        TotalSize += processedSize;
    }
    public void DetailComplete(long offset, int size)
    {
        DetailOffset = offset;
        DetailSize = size;

        TotalSize += size;
    }
    public void IndexComplete(long offset, int size)
    {
        IndexOffset = offset;
        IndexSize = size;

        TotalSize += size;
    }

    public IEnumerable<PZFile> GetConvertedFiles()
    {
        return _files.Values;
    }
    public IEnumerable<PZFolder> GetConvertedFolders()
    {
        return _folders.Values;
    }

    private void EnsureFolder(int id)
    {
        if (!_folders.ContainsKey(id) && id != Index.Root.Id)
        {
            var folder = Index.GetFolder(id);
            _folders.Add(folder.Id, new PZFolder(folder.Name, folder.Id, folder.Pid));
            EnsureFolder(folder.Pid);
        }
    }
    private void Check()
    {
        if (Index.IsEmpty)
        {
            throw new CreatorInvaildException("Index is empty");
        }
    }
}
