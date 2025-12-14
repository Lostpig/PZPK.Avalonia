using PZPK.Core.Extract;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PZPK.Desktop.Global;

public static class PackageManager
{
    public static bool HasOpened => Current != null;
    public static Package? Current { get; private set; }
    public static Package Open(string file, string password)
    {
        var stream = File.Open(file, FileMode.Open, FileAccess.Read);
        try
        {
            Current = Extractor.OpenPackage(stream, password);
        }
        catch
        {
            stream.Close();
            throw;
        }

        return Current;
    }

    [MemberNotNull(nameof(Current))]
    public static void Check()
    {
        if (Current == null)
        {
            throw new InvalidOperationException("Package not opened");
        }
    }
    public static void Close()
    {
        Current?.Dispose();
        Current = null;
    }
}
