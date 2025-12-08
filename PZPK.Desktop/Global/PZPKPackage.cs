using PZPK.Core;
using PZPK.Core.Extract;
using System.IO;

namespace PZPK.Desktop.Global;

public class PZPKPackage
{
    static public bool HasOpened => Current != null;
    public static PZPKPackage? Current { get; private set; }
    static public void Open(string file, string password)
    {
        Current = new PZPKPackage(file, password);
    }

    public Package Package { get; init; }
    public PZDetail Detail => Package.Detail;
    public PZHeader Header => Package.Header;
    public PackageIndex Index => Package.Index;
    public PZFolder Root => Index.Root;

    private PZPKPackage(string file, string password)
    {
        var stream = File.Open(file, FileMode.Open, FileAccess.Read);

        try
        {
            Package = Extractor.OpenPackage(stream, password);
        }
        catch
        {
            stream.Close();
            throw;
        }
    }

    public void Close()
    {
        Package.Dispose();
        Current = null;
    }
}
