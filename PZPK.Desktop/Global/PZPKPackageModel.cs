using PZPK.Core;
using PZPK.Core.Extract;
using System.IO;

namespace PZPK.Desktop.Global;

public class PZPKPackageModel
{
    static public bool HasOpened => Current != null;
    public static PZPKPackageModel? Current { get; private set; }
    static public void Open(string file, string password)
    {
        Current = new PZPKPackageModel(file, password);
    }

    public Package Package { get; init; }
    public PZDetail Detail => Package.Detail;
    public PZHeader Header => Package.Header;
    public PackageIndex Index => Package.Index;
    public PZFolder Root => Index.Root;

    private PZPKPackageModel(string file, string password)
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
