namespace PZPK.Core;

// interface
public interface IPZItem
{
    public string Name { get; }
    public int Id { get; }
    public int Pid { get; }
}
public interface IPZFile : IPZItem
{
    public long Size { get; }
    public string Extension { get; }
}
public interface IPZFolder : IPZItem;

// record
public record PZHeader(int Version,
        PZType Type,
        byte[] Sign,
        byte[] PasswordCheck,
        DateTime CreateTime,
        long FileSize,
        int BlockSize,
        int IndexSize,
        long IndexOffset,
        int DetailSize,
        long DetailOffset);

public record PZDetail(string Name, string Description, IReadOnlyList<string> Tags);

public record PZFile(string Name, int Id, int Pid, long Offset, long Size, long OriginSize) : IPZFile
{
    public string Extension { get; init; } = Path.GetExtension(Name);
}

public record PZFolder(string Name, int Id, int Pid) : IPZFolder;

public record PZIndexFolder(string Name, int Id, int Pid) : IPZFolder;

public record PZIndexFile(string Name, int Id, int Pid, string Source, long Size) : IPZFile
{
    public string Extension { get; init; } = Path.GetExtension(Name).ToLower();
}

public record PZNote(int Id, string Title, int Offset, int Size);