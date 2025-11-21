using PZPK.Core.Exceptions;
using PZPK.Core.Utility;
using System.Diagnostics;

namespace PZPK.Core.Crypto;

/**
 * V1 及 V2 版本
 */
internal class PZCryptoV2 : IPZCrypto
{
    private readonly PZCryptoBase _base;
    private readonly byte[] IV;
    public byte[] Key => _base.Key;

    public PZCryptoV2(byte[] key)
    {
        byte[] iv = new byte[16];
        byte[] ivHash = HashHelper.Sha256(key);
        Array.Copy(ivHash, 0, iv, 0, iv.Length);
        IV = iv;

        _base = new(key);
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

        var decryptedBytes = _base.Decrypt(input, output, IV);
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

        var decryptedBytes = _base.Decrypt(bytes, output, IV);
        return decryptedBytes;
    }

    public long EncryptStream(Stream source, Stream destination, Action<long, long>? progress = default)
    {
        throw new OldVersionEncryptException();
    }
    public long DecryptStream(Stream source, long offset, long length, Stream destination, Action<long, long>? progress = default)
    {
        ProgressStream partStream = new(source, offset, length, progress);
        return _base.DecryptStream(partStream, destination, IV);
    }

    public PZFileStream CreatePZFileStream(FileStream source, PZFile file)
    {
        throw new FileVersionNotCompatiblityException(2, "PZFileStream need pack version >= 11");
    }
    public void Dispose()
    {
        _base.Dispose();
    }
}

