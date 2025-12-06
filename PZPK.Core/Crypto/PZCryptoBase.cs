using System.Security.Cryptography;
using PZPK.Core.Utility;

namespace PZPK.Core.Crypto;

internal class PZCryptoBase : IDisposable
{
    public const int MaxByteDataSize = 1024 * 1024 * 256;  // 256 MB
    public readonly byte[] Key;
    public readonly Aes Crypto;

    public PZCryptoBase(byte[] key)
    {
        Key = key;
        Crypto = PZCrypto.CreateAes();
        Crypto.Key = key;
    }

    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> iv)
    {
        output.Clear();

        var encryptedBytes = Crypto.EncryptCbc(input, iv, output, Crypto.Padding);
        return encryptedBytes;
    }
    public int Decrypt(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> iv)
    {
        output.Clear();

        var decryptedBytes = Crypto.DecryptCbc(input, iv, output, Crypto.Padding);
        return decryptedBytes;
    }

    public int DecryptWithIV(ReadOnlySpan<byte> input, Span<byte> output)
    {
        output.Clear();

        var ivPart = input.Slice(0, 16);

        var contentPart = input.Slice(16);
        var written = Crypto.DecryptCbc(contentPart, ivPart, output, Crypto.Padding);

        return written;
    }
    public int EncryptWithIV(ReadOnlySpan<byte> input, Span<byte> output)
    {
        output.Clear();

        var ivPart = output.Slice(0, 16);
        PZCrypto.GenerateIV(ivPart);

        var contentPart = output.Slice(16);
        var encryptedBytes = Crypto.EncryptCbc(input, ivPart, contentPart, Crypto.Padding);

        return encryptedBytes + 16;
    }

    public long EncryptStream(Stream source, Stream destination, byte[] iv)
    {
        long originPositon = destination.Position;
        using ICryptoTransform encryptor = Crypto.CreateEncryptor(Key, iv);
        using CryptoStream encryptStream = new(source, encryptor, CryptoStreamMode.Read);

        destination.Write(iv, 0, iv.Length);
        encryptStream.CopyTo(destination);

        return destination.Position - originPositon;
    }
    public long DecryptStream(Stream source, Stream destination, byte[] iv)
    {
        long originPositon = destination.Position;

        using Aes crypto = PZCrypto.CreateAes();
        using ICryptoTransform decryptor = crypto.CreateDecryptor(Key, iv);
        using CryptoStream decryptStream = new(destination, decryptor, CryptoStreamMode.Write, true);

        source.CopyTo(decryptStream);

        return destination.Position - originPositon;
    }

    public static int ComputeEncryptedBlockSize(int blockSize, bool hasIV = true)
    {
        int encryptedBlockSize = 16 - (blockSize % 16) + blockSize;
        if (hasIV) encryptedBlockSize += 16;
        return encryptedBlockSize;
    }
    public static int ComputeDecryptedDataSize(int dataSize, bool hasIV = true)
    {
        int decryptedDataSize = dataSize - (16 - (dataSize % 16));
        if (hasIV) decryptedDataSize -= 16;
        return decryptedDataSize;
    }

    public long EncryptStreamBlock(Stream source, Stream destination, int blockSize)
    {
        StreamBlockWrapper wrapper = new(source, 0, source.Length, blockSize);
        BlockReader reader = new(wrapper);

        Span<byte> input = new byte[blockSize];
        Span<byte> output = new byte[global::PZPK.Core.Crypto.PZCryptoBase.ComputeEncryptedBlockSize(blockSize)];
        long totalWritten = 0;

        int readedBytes;
        int writtenBytes;
        while (!reader.End)
        {
            readedBytes = reader.ReadNext(input);
            var readed = input.Slice(0, readedBytes);

            writtenBytes = EncryptWithIV(readed, output);
            var written = output.Slice(0, writtenBytes);

            destination.Write(written);

            totalWritten += writtenBytes;
        }
        return totalWritten;
    }
    public long DecryptStreamBlock(Stream source, Stream destination, int blockSize)
    {
        using Aes crypto = PZCrypto.CreateAes();
        crypto.Key = Key;

        var encryptedBlockSize = ComputeEncryptedBlockSize(blockSize);
        StreamBlockWrapper wrapper = new(source, 0, source.Length, encryptedBlockSize);
        BlockReader reader = new(wrapper);

        Span<byte> input = new byte[reader.BlockSize];
        Span<byte> output = new byte[blockSize];
        long totalWritten = 0;

        int readedBytes;
        int writtenBytes;
        while (!reader.End)
        {
            readedBytes = reader.ReadNext(input);
            var readed = input.Slice(0, readedBytes);

            writtenBytes = DecryptWithIV(readed, output);
            var written = output.Slice(0, writtenBytes);

            destination.Write(written);

            totalWritten += writtenBytes;
        }
        return totalWritten;
    }

    public void Dispose()
    {
        Key.AsSpan().Clear(); // Clear the key in memory
        Crypto.Dispose();
        GC.SuppressFinalize(this);
    }
}