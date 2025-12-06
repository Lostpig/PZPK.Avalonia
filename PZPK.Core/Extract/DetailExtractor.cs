using PZPK.Core.Crypto;
using System.Text;

namespace PZPK.Core.Extract;

internal static class DetailExtractor
{
    public static PZDetail ExtractDetail(PZHeader header, IPZCrypto crypto, FileStream stream)
    {
        return header.Version switch
        {
            20 => ExtractDetailV20(header, crypto, stream),
            _ => GetEmptyDetail(),
        };
    }

    private static PZDetail GetEmptyDetail()
    {
        return new PZDetail("", "", []);
    }
    private static PZDetail ExtractDetailV20(PZHeader header, IPZCrypto crypto, FileStream stream)
    {
        Span<byte> buffer = new byte[header.DetailSize];

        using MemoryStream memory = new();
        crypto.DecryptStream(stream, header.DetailOffset, header.DetailSize, memory);
        using BinaryReader br = new(memory);

        br.BaseStream.Seek(0, SeekOrigin.Begin);
        int nameLength = br.ReadInt32();
        var nameBuffer = buffer[..nameLength];
        br.Read(nameBuffer);
        var name = Encoding.UTF8.GetString(nameBuffer);

        int descLength = br.ReadInt32();
        var descBuffer = buffer[..descLength];
        br.Read(descBuffer);
        var desc = Encoding.UTF8.GetString(descBuffer);

        int tagsLength = br.ReadInt32();
        var tagsBuffer = buffer[..tagsLength];
        br.Read(tagsBuffer);
        var tags = Encoding.UTF8.GetString(tagsBuffer);

        var tagList = tags.Split('|').ToList();

        return new PZDetail(name, desc, tagList);
    }
}
