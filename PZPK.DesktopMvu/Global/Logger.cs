using System.Diagnostics;
using System.IO;
using System.Text;

namespace PZPK.Desktop.Global;

internal class Logger
{
    static private Logger? _instance;
    static public Logger Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public string LogFile { get; set; } = "";
    public void Initialize(string logfile)
    {
        LogFile = logfile;
    }
    public void Log(string message)
    {
#if DEBUG
        LogDebug(message);
#else
        LogRelease(message);
#endif
    }
    private void LogRelease(string message)
    {
        if (!string.IsNullOrWhiteSpace(LogFile))
        {
            try
            {
                File.AppendAllText(LogFile, message, Encoding.UTF8);
            }
            catch
            {
                // Log failed
            }
        }
    }
    private static void LogDebug(string message)
    {
        Debug.WriteLine(message);
    }
}
