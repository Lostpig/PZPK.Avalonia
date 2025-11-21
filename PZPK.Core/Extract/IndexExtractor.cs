using PZPK.Core.Crypto;
using System.Text;
using PZPK.Core.Utility;

namespace PZPK.Core.Extract;

internal static class IndexExtractor
{
    public static PackageIndex ExtractIndex(PZHeader header, IPZCrypto crypto, FileStream stream)
    {
        Span<byte> indexBytes = new byte[header.IndexSize];
        stream.Seek(header.IndexOffset, SeekOrigin.Begin);
        stream.ReadExactly(indexBytes);
        Span<byte> buffer = new byte[header.IndexSize];

        var length = crypto.Decrypt(indexBytes, buffer);
        var decrypted = buffer[..length];

        return header.Version switch
        {
            1 => ExtractIndexV1(decrypted),
            2 or 4 => ExtractIndexV2(decrypted),
            11 or 12 or 20 => ExtractIndexV11(decrypted),
            _ => throw new Exceptions.FileVersionNotCompatiblityException(header.Version)
        };
    }

    // V1
    private static PackageIndex ExtractIndexV1(ReadOnlySpan<byte> buffer)
    {
        int foldersLen = BitConverter.ToInt32(buffer[..4]);
        int filesLen = BitConverter.ToInt32(buffer[4..8]);

        ReadOnlySpan<byte> foldersBuffer = buffer.Slice(8, foldersLen);
        ReadOnlySpan<byte> filesBuffer = buffer.Slice(8 + foldersLen, filesLen);

        var folders = ExtractFoldersV1(foldersBuffer);
        var files = ExtractFilesV1(filesBuffer);

        return new PackageIndex(folders, files);
    }
    private static Dictionary<int, PZFolder> ExtractFoldersV1(ReadOnlySpan<byte> buffer)
    {
        Dictionary<int, PZFolder> folders = [];

        int position = 0;
        while (position < buffer.Length)
        {
            int partLength = BitConverter.ToInt32(buffer.Slice(position, 4));
            var partBuffer = buffer.Slice(position + 4, partLength);

            int id = BitConverter.ToInt32(partBuffer[..4]);
            int pid = BitConverter.ToInt32(partBuffer[4..8]);
            string name = Encoding.UTF8.GetString(partBuffer[8..]);
            PZFolder folder = new(name, id, pid);
            folders.Add(id, folder);

            position += partLength + 4;
        }

        return folders;
    }
    private static Dictionary<int, PZFile> ExtractFilesV1(ReadOnlySpan<byte> buffer)
    {
        Dictionary<int, PZFile> files = [];
        Counter idCounter = new(640000);

        int position = 0;
        while (position < buffer.Length)
        {
            int partLength = BitConverter.ToInt32(buffer.Slice(position, 4));
            var partBuffer = buffer.Slice(position + 4, partLength);

            int pid = BitConverter.ToInt32(partBuffer[..4]);
            long offset = BitConverter.ToInt64(partBuffer[4..12]);
            int size = BitConverter.ToInt32(partBuffer[12..16]);
            string name = Encoding.UTF8.GetString(partBuffer[16..]);

            var id = idCounter.Next();
            PZFile file = new(name, id, pid, offset, size, size);
            files.Add(id, file);

            position += partLength + 4;
        }

        return files;
    }

    // V2
    private static PackageIndex ExtractIndexV2(ReadOnlySpan<byte> buffer)
    {
        int foldersLen = BitConverter.ToInt32(buffer[..4]);
        int filesLen = BitConverter.ToInt32(buffer[4..8]);

        ReadOnlySpan<byte> foldersBuffer = buffer.Slice(8, foldersLen);
        ReadOnlySpan<byte> filesBuffer = buffer.Slice(8 + foldersLen, filesLen);

        var folders = ExtractFoldersV1(foldersBuffer);
        var files = ExtractFilesV2(filesBuffer);

        return new PackageIndex(folders, files);
    }
    private static Dictionary<int, PZFile> ExtractFilesV2(ReadOnlySpan<byte> buffer)
    {
        Dictionary<int, PZFile> files = [];
        Counter idCounter = new(640000);

        int position = 0;
        while (position < buffer.Length)
        {
            int partLength = BitConverter.ToInt32(buffer.Slice(position, 4));
            var partBuffer = buffer.Slice(position + 4, partLength);

            int pid = BitConverter.ToInt32(partBuffer[..4]);
            long offset = BitConverter.ToInt64(partBuffer[4..12]);
            long size = BitConverter.ToInt64(partBuffer[12..20]);
            string name = Encoding.UTF8.GetString(partBuffer[20..]);

            var id = idCounter.Next();
            PZFile file = new(name, id, pid, offset, size, size);
            files.Add(id, file);

            position += partLength + 4;
        }

        return files;
    }

    // V11
    private static PackageIndex ExtractIndexV11(ReadOnlySpan<byte> buffer)
    {
        int foldersLen = BitConverter.ToInt32(buffer[..4]);
        int filesLen = BitConverter.ToInt32(buffer[4..8]);

        ReadOnlySpan<byte> foldersBuffer = buffer.Slice(8, foldersLen);
        ReadOnlySpan<byte> filesBuffer = buffer.Slice(8 + foldersLen, filesLen);

        var folders = ExtractFoldersV11(foldersBuffer);
        var files = ExtractFilesV11(filesBuffer);

        return new PackageIndex(folders, files);
    }
    private static Dictionary<int, PZFolder> ExtractFoldersV11(ReadOnlySpan<byte> buffer)
    {
        Dictionary<int, PZFolder> folders = [];

        int position = 0;
        while (position < buffer.Length)
        {
            int partLength = BitConverter.ToInt32(buffer.Slice(position, 4));
            var partBuffer = buffer.Slice(position + 4, partLength - 4);

            int id = BitConverter.ToInt32(partBuffer[..4]);
            int pid = BitConverter.ToInt32(partBuffer[4..8]);
            string name = Encoding.UTF8.GetString(partBuffer[8..]);
            PZFolder folder = new(name, id, pid);
            folders.Add(id, folder);

            position += partLength;
        }

        return folders;
    }
    private static Dictionary<int, PZFile> ExtractFilesV11(ReadOnlySpan<byte> buffer)
    {
        Dictionary<int, PZFile> files = [];

        int position = 0;
        while (position < buffer.Length)
        {
            int partLength = BitConverter.ToInt32(buffer.Slice(position, 4));
            var partBuffer = buffer.Slice(position + 4, partLength - 4);

            int id = BitConverter.ToInt32(partBuffer[..4]);
            int pid = BitConverter.ToInt32(partBuffer[4..8]);
            long offset = BitConverter.ToInt64(partBuffer[8..16]);
            long size = BitConverter.ToInt64(partBuffer[16..24]);
            long originSize = BitConverter.ToInt64(partBuffer[24..32]);
            string name = Encoding.UTF8.GetString(partBuffer[32..]);
            PZFile file = new(name, id, pid, offset, size, originSize);
            files.Add(id, file);

            position += partLength;
        }

        return files;
    }
}
