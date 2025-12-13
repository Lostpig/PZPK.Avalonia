using System.Collections.Generic;
using System.Text;

namespace PZPK.Core;

public interface IPZIndex<TFolder, TFile> where TFolder : IPZFolder where TFile : IPZFile
{
    TFolder GetFolder(int id);
    TFile GetFile(int id);
    List<TFile> GetFiles(TFolder folder, bool recursive);
    List<TFolder> GetFolders(TFolder parent, bool recursive);
    List<TFile> GetAllFiles();
    List<TFolder> GetAllFolders();
    TFolder Root { get; }
    int FilesCount { get; }
}
public abstract class IndexBase<TFolder, TFile> : IPZIndex<TFolder, TFile> where TFolder : IPZFolder where TFile : IPZFile
{
    protected abstract IDictionary<int, TFile> Files { get; }
    protected abstract IDictionary<int, TFolder> Folders { get; }
    public abstract TFolder Root { get; }
    public abstract int FilesCount { get; }

    public TFile GetFile(int id)
    {
        if (!Files.ContainsKey(id))
        {
            throw new Exceptions.PZFileNotFoundException("", id);
        }

        return Files[id];
    }
    public TFolder GetFolder(int id)
    {
        if (id == Root.Id) return Root;

        if (!Folders.ContainsKey(id))
        {
            throw new Exceptions.PZFolderNotFoundException("", id);
        }

        return Folders[id];
    }
    public List<TFile> GetFiles(TFolder folder, bool recursive)
    {
        var result = Files.Values.Where(f => f.Pid == folder.Id).ToList();
        if (recursive)
        {
            var subFolders = GetFolders(folder, true);
            foreach (var f in subFolders)
            {
                var subFiles = GetFiles(f, false);
                if (subFiles.Count > 0)
                {
                    result.AddRange(subFiles);
                }
            }
        }

        return result;
    }
    public List<TFolder> GetFolders(TFolder parent, bool recursive)
    {
        var result = Folders.Values.Where(f => f.Pid == parent.Id).ToList();
        if (recursive)
        {
            foreach (var f in result)
            {
                var subFolders = GetFolders(f, true);
                if (subFolders.Count > 0)
                {
                    result.AddRange(subFolders);
                }
            }
        }

        return result;
    }
    public List<TFile> GetAllFiles()
    {
        return Files.Values.ToList();
    }
    public List<TFolder> GetAllFolders()
    {
        return Folders.Values.ToList();
    }
    public List<IPZItem> GetItems(TFolder parent, bool recursive)
    {
        var folders = GetFolders(parent, recursive);
        var files = GetFiles(parent, recursive);

        return [.. folders, ..files];
    }

    public Stack<TFolder> GetFolderResolveStack(TFolder folder, TFolder? resolveFolder = default)
    {
        Stack<TFolder> stack = new();
        TFolder current = folder;

        while (current.Id != Constants.IndexRootId)
        {
            stack.Push(current);
            if (resolveFolder?.Id == current.Id)
            {
                break;
            }

            current = GetFolder(current.Pid);
        }

        return stack;
    }
    public string GetResolvePath(TFolder folder, TFolder resolveFolder)
    {
        var stack = GetFolderResolveStack(folder, resolveFolder);

        StringBuilder sb = new();
        while (stack.Count > 0)
        {
            var f = stack.Pop();
            sb.Append(f.Name + Path.DirectorySeparatorChar);
        }
        return sb.ToString();
    }
    public string GetResolvePath(TFile file, TFolder resolveFolder)
    {
        var parent = GetFolder(file.Pid);
        string folderNames = GetResolvePath(parent, resolveFolder);
        return folderNames + file.Name;
    }
    public string GetFullPath(TFolder folder)
    {
        return GetResolvePath(folder, Root);
    }
    public string GetFullPath(TFile file)
    {
        return GetResolvePath(file, Root);
    }
}
