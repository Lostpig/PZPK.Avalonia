namespace PZPK.Core.Extract;

public class PackageIndex : IndexBase<PZFolder, PZFile>
{
    private readonly Dictionary<int, PZFolder> _folders = new();
    private readonly Dictionary<int, PZFile> _files = new();
    private readonly PZFolder _root;

    protected override IDictionary<int, PZFile> Files { get => _files; }
    protected override IDictionary<int, PZFolder> Folders { get => _folders; }
    public override PZFolder Root { get => _root; }
    public override int FilesCount { get => _files.Count; }

    internal PackageIndex(Dictionary<int, PZFolder> folders, Dictionary<int, PZFile> files)
    {
        _root = new PZFolder("", Constants.IndexRootId, 0);
        _folders = folders;
        _files = files;
    }
}
