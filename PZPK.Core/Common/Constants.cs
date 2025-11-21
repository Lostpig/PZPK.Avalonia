using PZPK.Core.Utility;

namespace PZPK.Core;

public static class Constants
{
    internal const int MemInitLength = 65536;
    public const int Version = 20;
    internal const int IndexRootId = 10000;

    public static string GetTypeIDText(PZType pztype)
    {
        return pztype switch
        {
            PZType.Package => "PZPK-Package",
            PZType.Note => "PZPK-Note",
            _ => throw new ArgumentOutOfRangeException(nameof(pztype), pztype, null)
        };
    }
    public static string PZTypeToExtension(PZType pztype)
    {
        return pztype switch
        {
            PZType.Package => ".pzpk",
            PZType.Note => ".pznt",
            _ => throw new ArgumentOutOfRangeException(nameof(pztype), pztype, null)
        };
    }
    public static PZType ExtensionToPZType(string extension)
    {
        return extension.ToLower() switch
        {
            ".pzpk" => PZType.Package,
            ".pznt" => PZType.Note,
            _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, null)
        };
    }

    public static byte[] GetTypeHashSign(PZType type)
    {
        var id = GetTypeIDText(type);
        return HashHelper.Sha256(id);
    }
    public static int GetTypeHashSign(PZType type, Span<byte> dest)
    {
        var id = GetTypeIDText(type);
        return HashHelper.Sha256(id, dest);
    }
    public static string GetTypeHashSignHex(PZType type)
    {
        var id = GetTypeIDText(type);
        return HashHelper.Sha256Hex(id);
    }

    public static class Sizes
    {
        public const int t_4KB = 1024 * 4;
        public const int t_64KB = 1024 * 64;
        public const int t_256KB = 1024 * 256;
        public const int t_1MB = 1024 * 1024;
        public const int t_4MB = 1024 * 1024 * 4;
        public const int t_8MB = 1024 * 1024 * 8;
        public const int t_16MB = 1024 * 1024 * 16;
        public const int t_64MB = 1024 * 1024 * 64;
        public const int t_256MB = 1024 * 1024 * 256;
        public const int t_1GB = 1024 * 1024 * 1024; 
    }
}
