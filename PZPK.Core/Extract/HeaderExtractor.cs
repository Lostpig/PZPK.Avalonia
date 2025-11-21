using PZPK.Core.Exceptions;
using PZPK.Core.Crypto;
using PZPK.Core.Utility;
using System;

namespace PZPK.Core.Extract;

internal static class HeaderExtractor
{
    public static PZHeader ExtractHeader(FileStream stream)
    {
        using BinaryReader br = new(stream, System.Text.Encoding.UTF8, true);
        br.BaseStream.Seek(0, SeekOrigin.Begin);
        int version = br.ReadInt32();

        return version switch
        {
            1 or 2 or 4 => ExtractHeaderV2(br),
            11 => ExtractHeaderV11(br),
            12 => ExtractHeaderV12(br),
            20 => ExtractHeaderV20(br),
            _ => throw new FileVersionNotCompatiblityException(version, $"Unsupported version: {version}"),
        };
    }

    public static void CheckPassword(IPZCrypto crypto, PZHeader header)
    {
        switch (header.Version)
        {
            case 20 : 
                CheckPasswordV20(crypto, header);
                break;
            default:
                CheckPasswordV2(crypto, header);
                break;
        };
    }
    private static void CheckPasswordV2(IPZCrypto crypto, PZHeader header)
    {
        var hash = Compatible.CreateKeyHash(crypto.Key);
        if (!Utils.CompareBytes(hash, header.PasswordCheck))
        {
            throw new PZPasswordIncorrectException();
        }
    }
    private static void CheckPasswordV20(IPZCrypto crypto, PZHeader header)
    {
        Span<byte> pwCheck = stackalloc byte[64];
        header.PasswordCheck.CopyTo(pwCheck);

        crypto.Encrypt(header.Sign, pwCheck[16..], pwCheck[..16]);
        if (!Utils.CompareBytes(pwCheck, header.PasswordCheck))
        {
            throw new PZPasswordIncorrectException();
        }
    }

    private static PZHeader ExtractHeaderV2(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        int version = br.ReadInt32();
        byte[] sign = br.ReadBytes(32);
        byte[] passwordHash = br.ReadBytes(32);

        int detailLength = br.ReadInt32();
        br.ReadBytes(detailLength);
        long indexOffset = br.ReadInt64();

        return new PZHeader(
                    Version: version,
                    Type: PZType.Package,
                    Sign: sign,
                    PasswordCheck: passwordHash,
                    CreateTime: DateTime.MinValue,
                    FileSize: br.BaseStream.Length,
                    BlockSize: 0,
                    IndexSize: (int)(br.BaseStream.Length - indexOffset),
                    IndexOffset: indexOffset,
                    DetailSize: 0,
                    DetailOffset: 0
                );
    }
    private static PZHeader ExtractHeaderV11(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        int version = br.ReadInt32();
        byte[] sign = br.ReadBytes(32);
        byte[] passwordHash = br.ReadBytes(32);
        long createTime = br.ReadInt64();
        long fullSize = br.ReadInt64();
        int blockSize = br.ReadInt32();
        int indexSize = br.ReadInt32();

        return new PZHeader(
                    Version: version,
                    Type: PZType.Package,
                    Sign: sign,
                    PasswordCheck: passwordHash,
                    CreateTime: DateTime.FromBinary(createTime),
                    FileSize: fullSize,
                    BlockSize: blockSize,
                    IndexSize: indexSize,
                    IndexOffset: 92L,
                    DetailSize: 0,
                    DetailOffset: 0
                );
    }
    private static PZHeader ExtractHeaderV12(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        int version = br.ReadInt32();
        byte[] sign = br.ReadBytes(32);
        byte[] passwordHash = br.ReadBytes(32);
        long createTime = br.ReadInt64();
        long fullSize = br.ReadInt64();
        int blockSize = br.ReadInt32();
        long indexOffset = br.ReadInt64();
        int indexSize = (int)(br.BaseStream.Length - indexOffset);

        return new PZHeader(
                    Version: version,
                    Type: PZType.Package,
                    Sign: sign,
                    PasswordCheck: passwordHash,
                    CreateTime: DateTime.FromBinary(createTime),
                    FileSize: fullSize,
                    BlockSize: blockSize,
                    IndexSize: indexSize,
                    IndexOffset: indexOffset,
                    DetailSize: 0,
                    DetailOffset: 0
                );
    }
    private static PZHeader ExtractHeaderV20(BinaryReader br)
    {
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        int version = br.ReadInt32();
        PZType type = (PZType)br.ReadInt32();
        byte[] sign = br.ReadBytes(32);
        byte[] passwordCheck = br.ReadBytes(64);
        long createTime = br.ReadInt64();
        long fullSize = br.ReadInt64();
        int blockSize = br.ReadInt32();
        long detailOffset = br.ReadInt64();
        int detailSize = br.ReadInt32();
        long indexOffset = br.ReadInt64();
        int indexSize = br.ReadInt32();

        return new PZHeader(
                    Version: version,
                    Type: type,
                    Sign: sign,
                    PasswordCheck: passwordCheck,
                    CreateTime: DateTime.FromBinary(createTime),
                    FileSize: fullSize,
                    BlockSize: blockSize,
                    IndexSize: indexSize,
                    IndexOffset: indexOffset,
                    DetailSize: detailSize,
                    DetailOffset: detailOffset
                );
    }
}
