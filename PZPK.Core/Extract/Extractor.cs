using PZPK.Core.Crypto;

namespace PZPK.Core.Extract;

public static class Extractor
{
    public static PZHeader ExtractHeader(FileStream stream)
    {
        return HeaderExtractor.ExtractHeader(stream);
    }
    public static Package OpenPackage(FileStream stream, string password)
    {
        return new Package(stream, password);
    }
}

