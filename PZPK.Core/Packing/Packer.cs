using PZPK.Core.Crypto;
using PZPK.Core.Exceptions;
using PZPK.Core.Utility;
using System.Diagnostics;
using System.Text;

namespace PZPK.Core.Packing;

public static class Packer
{
    const int HeaderSize = 148;

    private static void WriteContent(FileStream writer, IPZCrypto crypto, PackingContext context)
    {
        writer.Seek(HeaderSize, SeekOrigin.Begin);

        void currentProgress(long readed, long total)
        {
            context.FileProgress(readed, total);
        }

        long size = 0;
        long offset = 0;
        foreach (var file in context.Files)
        {
            using var fs = File.OpenRead(file.Source);
            using var progressStream = new ProgressStream(fs, 0, fs.Length, currentProgress);

            offset = writer.Position;
            string? newName = null;
            if (context.ImageResizer != null && context.ImageResizer.CheckIsImageFile(file))
            {
                using var imgStream = context.ImageResizer.ResizeImage(progressStream);
                size = crypto.EncryptStream(imgStream, writer);
                newName = Path.ChangeExtension(file.Name, context.ImageResizer.Extension);
            }
            else
            {
                size = crypto.EncryptStream(progressStream, writer);
            }

            Debug.Assert(size == writer.Position - offset);
            context.FileComplete(file, size, offset, newName);
        }
    }
    private static void WriteHeader(FileStream writer, IPZCrypto crypto, PackingContext context)
    {
        using BinaryWriter bw = new(writer, Encoding.Default, true);
        bw.Seek(0, SeekOrigin.Begin);

        bw.Write(Constants.Version);
        bw.Write((int)context.Options.Type);
        Span<byte> buffer = stackalloc byte[256];

        var signBuf = buffer[..32];
        Constants.GetTypeHashSign(context.Options.Type, buffer);
        bw.Write(signBuf);

        var pwcBuf = buffer.Slice(64, 64);
        var pwcLen = crypto.Encrypt(signBuf, pwcBuf);
        bw.Write(pwcBuf);
        Debug.Assert(pwcLen == 64);

        bw.Write(DateTime.Now.Ticks);
        bw.Write(context.TotalSize + HeaderSize);
        bw.Write(context.Options.BlockSize);
        bw.Write(context.DetailOffset);
        bw.Write(context.DetailSize);
        bw.Write(context.IndexOffset);
        bw.Write(context.IndexSize);
    }
    private static int WriteIndex(FileStream writer, IPZCrypto crypto, PackingContext context)
    {
        using MemoryStream memory = new();
        using BinaryWriter bw = new(memory);

        int foldersLength = 0;
        int filesLength = 0;
        bw.Seek(8, SeekOrigin.Begin);

        foreach (var folder in context.GetConvertedFolders())
        {
            byte[] name = Encoding.UTF8.GetBytes(folder.Name);
            bw.Write(12 + name.Length);

            bw.Write(folder.Id);
            bw.Write(folder.Pid);
            bw.Write(name);
            foldersLength += 12 + name.Length;
        }
        foreach (var file in context.GetConvertedFiles())
        {
            byte[] name = Encoding.UTF8.GetBytes(file.Name);
            bw.Write(36 + name.Length);

            bw.Write(file.Id);
            bw.Write(file.Pid);
            bw.Write(file.Offset);
            bw.Write(file.Size);
            bw.Write(file.OriginSize);
            bw.Write(name);
            filesLength += 36 + name.Length;
        }

        bw.Seek(0, SeekOrigin.Begin);
        bw.Write(foldersLength);
        bw.Write(filesLength);
        bw.Flush();

        Span<byte> encryptedBuf = new byte[PZCryptoBase.ComputeEncryptedBlockSize((int)memory.Length)];
        var encrypted = crypto.Encrypt(memory.ToArray(), encryptedBuf);
        writer.Write(encryptedBuf);
        return encrypted;
    }
    private static int WriteDetails(FileStream writer, IPZCrypto crypto, PackingOptions options)
    {
        using MemoryStream memory = new();
        using BinaryWriter bw = new(memory);

        byte[] name = Encoding.UTF8.GetBytes(options.Name);
        bw.Write(name.Length);
        bw.Write(name);

        byte[] desc = Encoding.UTF8.GetBytes(options.Description);
        bw.Write(desc.Length);
        bw.Write(desc);

        string tagsText = string.Join("|", options.Tags);
        byte[] tags = Encoding.UTF8.GetBytes(tagsText);
        bw.Write(tags.Length);
        bw.Write(tags);
        bw.Flush();

        Span<byte> encryptedBuf = new byte[PZCryptoBase.ComputeEncryptedBlockSize((int)memory.Length)];
        var encrypted = crypto.Encrypt(memory.ToArray(), encryptedBuf);
        writer.Write(encryptedBuf);
        return encrypted;
    }
    private static long ExcutePack(FileStream writer, PackingContext context)
    {
        byte[] key = PZCrypto.CreateKey(context.Options.Password);
        using var crypto = PZCrypto.Create(Constants.Version, key, context.Options.BlockSize);

        context.Start();

        WriteContent(writer, crypto, context);

        long detailsOffset = writer.Position;
        int detailsLength = WriteDetails(writer, crypto, context.Options);
        context.DetailComplete(detailsOffset, detailsLength);

        long indexOffset = writer.Position;
        int indexLength = WriteIndex(writer, crypto, context);
        context.IndexComplete(indexOffset, indexLength);

        Debug.Assert(context.TotalSize == writer.Position + HeaderSize);
        WriteHeader(writer, crypto, context);

        key.AsSpan().Clear();
        writer.Flush();
        return context.TotalSize + HeaderSize;
    }

    public static long Pack(string destination,
        IndexCreator index,
        PackingOptions options,
        IProgress<PZProgressState>? progress,
        IImageResizer? imageResizer)
    {
        if (File.Exists(destination))
        {
            throw new OutputFileAlreadyExistsException(destination);
        }

        using FileStream writer = new(destination, FileMode.Create, FileAccess.Write);
        PackingContext context = new(options, index, imageResizer, progress);
        return ExcutePack(writer, context);
    }
    public static Task<long> PackAsync(
        string destination, 
        IndexCreator index, 
        PackingOptions options, 
        IProgress<PZProgressState>? progress,
        IImageResizer? imageResizer,
        CancellationToken? cancelToken = null)
    {
        if (File.Exists(destination))
        {
            throw new OutputFileAlreadyExistsException(destination);
        }

        using FileStream writer = new(destination, FileMode.Create, FileAccess.Write);
        PackingContext context = new(options, index, imageResizer, progress);
        return Task.Run(() => ExcutePack(writer, context), cancelToken ?? CancellationToken.None);
    }
}
