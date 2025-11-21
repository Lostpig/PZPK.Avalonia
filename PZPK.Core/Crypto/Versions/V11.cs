using PZPK.Core.Utility;
using System.Diagnostics;

namespace PZPK.Core.Crypto;

/**
 * V11/V12/V20 版本
 */
internal class PZCryptoV11 : IPZCrypto
{
    private readonly PZCryptoBase _base;
    private readonly int _blockSize;
    public byte[] Key => _base.Key;

    public PZCryptoV11(byte[] key, int blockSize)
    {
        _base = new PZCryptoBase(key);
        _blockSize = blockSize;
    }

    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length > PZCryptoBase.MaxByteDataSize) { 
            throw new ArgumentException($"Input data size exceeds the maximum limit of {PZCryptoBase.MaxByteDataSize} bytes.", nameof(input));
        }

        var encryptedBytes = _base.EncryptWithIV(input, output);
        if (output.Length < encryptedBytes)
        {
            throw new ArgumentException("Output buffer is too small.", nameof(output));
        }
        return encryptedBytes;
    }
    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> iv)
    {
        if (input.Length > PZCryptoBase.MaxByteDataSize)
        {
            throw new ArgumentException($"Input data size exceeds the maximum limit of {PZCryptoBase.MaxByteDataSize} bytes.", nameof(input));
        }

        var encryptedBytes = _base.Encrypt(input, output, iv);
        if (output.Length < encryptedBytes)
        {
            throw new ArgumentException("Output buffer is too small.", nameof(output));
        }
        return encryptedBytes;
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

        var encryptedBlockSize = PZCryptoBase.ComputeEncryptedBlockSize(_blockSize);

        int totalDecrypted = 0;
        for (int offset = 0; offset < file.Size; offset += encryptedBlockSize)
        {
            var blockSize = (int)Math.Min(encryptedBlockSize, file.Size - offset);
            var decryptedBytes = _base.DecryptWithIV(bytes.Slice(offset, blockSize), output.Slice(totalDecrypted));
            totalDecrypted += decryptedBytes;
        }

        return totalDecrypted;
    }

    public long EncryptStream(Stream source, Stream destination, Action<long, long>? progress = default)
    {
        ProgressStream progressStream = new(source, 0, source.Length, progress);
        return _base.EncryptStreamBlock(progressStream, destination, _blockSize);
    }
    public long DecryptStream(Stream source, long offset, long length, Stream destination, Action<long, long>? progress = default)
    {
        ProgressStream progressStream = new(source, offset, length, progress);
        return _base.DecryptStreamBlock(progressStream, destination, _blockSize);
    }

    public PZFileStream CreatePZFileStream(FileStream source, PZFile file)
    {
        return new PZFileStream(file, source, _base, _blockSize);
    }

    public void Dispose()
    {
        _base.Dispose();
    }
}

