using PZPK.Core.Utility;

namespace PZPK.Core.Crypto;

public interface IPZCrypto : IDisposable
{
    internal byte[] Key { get; }

    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output);
    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> iv);
    public int Decrypt(ReadOnlySpan<byte> input, Span<byte> output);

    public long EncryptStream(Stream source, Stream target, Action<long, long>? progress = default);
    public long DecryptStream(Stream source, long offset, long length, Stream target, Action<long, long>? progress = default);

    public int DecryptFile(Stream source, PZFile file, Span<byte> output);
    public PZFileStream CreatePZFileStream(FileStream source, PZFile file);
}
