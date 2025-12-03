using PZPK.Core.Utility;

namespace PZPK.Core.Packing;

public class IndexCreator: IndexBase<PZIndexFolder, PZIndexFile>
{
    private readonly Counter _idCounter = new(Constants.IndexRootId + 1);
    private readonly Dictionary<int, PZIndexFolder> _folders = new();
    private readonly Dictionary<int, PZIndexFile> _files = new();
    private readonly PZIndexFolder _root;

    public override PZIndexFolder Root { get => _root; }
    public bool IsEmpty { get => _files.Count == 0; }
    public override int FilesCount { get => _files.Count; }

    protected override IDictionary<int, PZIndexFile> Files { get => _files; }
    protected override IDictionary<int, PZIndexFolder> Folders { get => _folders; }

    public IndexCreator()
    {
        _root = new PZIndexFolder("", Constants.IndexRootId, 0);
    }

    internal void CheckFolderExists(PZIndexFolder folder)
    {
        if (!_folders.ContainsKey(folder.Id) && folder.Id != Root.Id)
        {
            throw new Exceptions.PZFolderNotFoundException(folder.Name, folder.Id);
        }
    }
    internal void CheckFileName(string name, PZIndexFolder parent)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.EmptyStringException(nameof(name));
        }

        var files = GetFiles(parent, false);
        if (files.Exists(f => f.Name == name))
        {
            throw new Exceptions.DuplicateNameException(name);
        }
    }
    internal void CheckFolderName(string name, PZIndexFolder parent)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.EmptyStringException(nameof(name));
        }

        var folders = GetFolders(parent, false);
        if (folders.Exists(f => f.Name == name))
        {
            throw new Exceptions.DuplicateNameException(name);
        }
    }

    public long SumFilesSize()
    {
        long total = 0;
        foreach (var file in _files)
        {
            total += file.Value.Size;
        }
        return total;
    }

    // File Methods
    public PZIndexFile AddFile(string source, string name, PZIndexFolder parent)
    {
        FileInfo sourceInfo = new(source);
        if (!sourceInfo.Exists)
        {
            throw new FileNotFoundException(string.Empty, source);
        }

        CheckFolderExists(parent);
        CheckFileName(name, parent);

        int id = _idCounter.Next();
        PZIndexFile file = new(name, id, parent.Id, source, sourceInfo.Length);
        _files.Add(id, file);
        return file;
    }
    public PZIndexFile AddFile(FileInfo fi, PZIndexFolder parent, string? rename = null)
    {
        if (!fi.Exists)
        {
            throw new FileNotFoundException(string.Empty, fi.FullName);
        }
        rename = rename ?? fi.Name;

        CheckFolderExists(parent);
        CheckFileName(rename, parent);

        int id = _idCounter.Next();
        PZIndexFile file = new(rename, id, parent.Id, fi.FullName, fi.Length);
        _files.Add(id, file);
        return file;
    }

    public bool RemoveFile(PZIndexFile file)
    {
        return _files.Remove(file.Id);
    }
    public void MoveFile(PZIndexFile file, PZIndexFolder toFolder)
    {
        CheckFileName(file.Name, toFolder);
        var newFile = file with { Pid = toFolder.Id };
        _files[file.Id] = newFile;
    }
    public void RenameFile(PZIndexFile file, string newName)
    {
        if (newName == file.Name) return;

        PZIndexFolder folder = GetFolder(file.Pid);
        CheckFileName(newName, folder);

        var newFile = file with { Name = newName };
        _files[file.Id] = newFile;
    }

    // Folder Methods
    public PZIndexFolder AddFolder(string name, PZIndexFolder parent)
    {
        CheckFolderExists(parent);
        CheckFolderName(name, parent);

        int id = _idCounter.Next();
        PZIndexFolder folder = new(name, id, parent.Id);
        _folders.Add(id, folder);
        return folder;
    }
    public bool RemoveFolder(PZIndexFolder folder)
    {
        if (folder.Id == Constants.IndexRootId)
        {
            throw new ArgumentException("Cannot remove root folder", nameof(folder));
        }

        List<PZIndexFile> files = GetFiles(folder, true);
        foreach (var file in files)
        {
            _files.Remove(file.Id);
        }
        List<PZIndexFolder> subFolders = GetFolders(folder, true);
        foreach (var subFolder in subFolders)
        {
            _folders.Remove(subFolder.Id);
        }

        return _folders.Remove(folder.Id);
    }
    public void MoveFolder(PZIndexFolder folder, PZIndexFolder toFolder)
    {
        if (folder.Id == Constants.IndexRootId)
        {
            throw new ArgumentException("Cannot move root folder", nameof(folder));
        }

        CheckFolderName(folder.Name, toFolder);
        var newFolder = folder with { Pid = toFolder.Id };
        _folders[folder.Id] = newFolder;
    }
    public void RenameFolder(PZIndexFolder folder, string newName)
    {
        if (folder.Id == Constants.IndexRootId)
        {
            throw new ArgumentException("Cannot rename root folder", nameof(folder));
        }

        if (newName == folder.Name) return;
        PZIndexFolder parent = GetFolder(folder.Pid);
        CheckFolderName(newName, parent);

        var newFolder = folder with { Name = newName };
        _folders[folder.Id] = newFolder;
    }

    public void Reset()
    {
        _folders.Clear();
        _files.Clear();
        _idCounter.Reset();
    }

    public void Clear()
    {
        _folders.Clear();
        _files.Clear();
    }
}
