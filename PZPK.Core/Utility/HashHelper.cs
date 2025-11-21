using System.Text;
using System.Security.Cryptography;

namespace PZPK.Core.Utility;

public static class HashHelper
{
    public static byte[] Sha256(string text)
    {
        Span<byte> result = stackalloc byte[SHA256.HashSizeInBytes];
        Sha256(text, result);

        return result.ToArray();
    }
    public static byte[] Sha256(ReadOnlySpan<byte> source)
    {
        Span<byte> result = stackalloc byte[SHA256.HashSizeInBytes];
        Sha256(source, result);

        return result.ToArray();
    }
    public static int Sha256(string text, Span<byte> dest)
    {
        Span<byte> bytes = Encoding.UTF8.GetBytes(text);
        return Sha256(bytes, dest);
    }
    public static int Sha256(ReadOnlySpan<byte> source, Span<byte> dest)
    {
        return SHA256.HashData(source, dest);
    }

    public static string Sha256Hex(string text)
    {
        Span<byte> hashBytes = Sha256(text);
        return Convert.ToHexString(hashBytes);
    }
    public static string Sha256Hex(byte[] source)
    {
        Span<byte> hashBytes = Sha256(source);
        return Convert.ToHexString(hashBytes);
    }
}
