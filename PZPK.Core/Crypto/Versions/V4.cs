using PZPK.Core.Exceptions;
using PZPK.Core.Utility;
using System.Diagnostics;

namespace PZPK.Core.Crypto;

/**
 * V4 版本
 */
internal class PZCryptoV4 : IPZCrypto
{
    private readonly PZCryptoBase _base;
    public byte[] Key => _base.Key;

    public PZCryptoV4(byte[] key)
    {
        _base = new PZCryptoBase(key);
    }

    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output)
    {
        throw new OldVersionEncryptException();
    }
    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> iv)
    {
        throw new OldVersionEncryptException();
    }
    public int Decrypt(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length > PZCryptoBase.MaxByteDataSize)
        {
            throw new ArgumentException($"Input data size exceeds the maximum limit of {PZCryptoBase.MaxByteDataSize} bytes.", nameof(input));
        }

        var decryptedBytes = _base.DecryptWithIV(input, output);
        if (output.Length < decryptedBytes)
        {
            throw new ArgumentException("Output buffer is too small.", nameof(output));
        }
        return decryptedBytes;
    }
    public int DecryptFile(Stream source, PZFile file, Span<byte> output)
    {
        if (file.Size > PZCryptoBase.MaxByteDataSize)
        {
            throw new ArgumentException($"Input data size exceeds the maximum limit of {PZCryptoBase.MaxByteDataSize} bytes.", nameof(file));
        }
        if (output.Length < file.OriginSize)
        {
            throw new ArgumentException("Output buffer is too small.", nameof(output));
        }

        Span<byte> bytes = new byte[file.Size];
        source.Position = file.Offset;
        source.ReadExactly(bytes);

        var decryptedBytes = _base.DecryptWithIV(bytes, output);
        return decryptedBytes;
    }

    public long EncryptStream(Stream source, Stream destination, Action<long, long>? progress = default)
    {
        throw new OldVersionEncryptException();
    }
    public long DecryptStream(Stream source, long offset, long length, Stream destination, Action<long, long>? progress = default)
    {
        byte[] IV = new byte[16];
        source.Seek(offset, SeekOrigin.Begin);
        source.ReadExactly(IV, 0, IV.Length);

        ProgressStream partStream = new(source, offset + 16, length - 16, progress);
        return _base.DecryptStream(partStream, destination, IV);
    }

    public PZFileStream CreatePZFileStream(FileStream source, PZFile file)
    {
        throw new FileVersionNotCompatiblityException(4, "PZFileStream need pack version >= 11");
    }
    public void Dispose()
    {
        _base.Dispose();
    }
}
