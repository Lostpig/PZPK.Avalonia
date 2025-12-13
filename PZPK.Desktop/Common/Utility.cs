namespace PZPK.Desktop.Common;

internal class Utility
{
    static public string ComputeFileSize(double size)
    {
        int count = 0;
        double n = size / 1024;
        string[] suffix = [" KB", " MB", " GB", " TB", " PB"];

        while (n > 1024 && count < suffix.Length)
        {
            n /= 1024;
            count++;
        }

        return n.ToString("f1") + suffix[count];
    }
    static public string ComputeFileSize(long size)
    {
        return ComputeFileSize((double)size);
    }

    static public double ComputePercent(int value, int total)
    {
        return ComputePercent((double)value, (double)total);
    }
    static public double ComputePercent(long value, long total)
    {
        return ComputePercent((double)value, (double)total);
    }
    static public double ComputePercent(double value, double total)
    {
        return value / total * 100;
    }
}
