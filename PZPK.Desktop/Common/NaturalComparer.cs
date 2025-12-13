using PZPK.Core;
using System.Text.RegularExpressions;

namespace PZPK.Desktop.Common;

public partial class NaturalComparer : IComparer<string>
{
    private static NaturalComparer? _instance;
    public static NaturalComparer Instance { get { return _instance ??= new NaturalComparer(); } }

    public int Compare(string? x, string? y)
    {
        var regex = NaturalSortRegex();

        // run the regex on both strings
        var xRegexResult = regex.Match(x ?? "");
        var yRegexResult = regex.Match(y ?? "");

        // check if they are both numbers
        if (xRegexResult.Success && yRegexResult.Success)
        {
            return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
        }

        // otherwise return as string comparison
        return x?.CompareTo(y) ?? 0;
    }

    [GeneratedRegex("^(d+)")]
    private static partial Regex NaturalSortRegex();
}

internal sealed class NaturalPZItemComparer : IComparer<IPZItem>
{
    private static NaturalPZItemComparer? _instance;
    public static NaturalPZItemComparer Instance { get { return _instance ??= new NaturalPZItemComparer(); } }
    public int Compare(IPZItem? a, IPZItem? b)
    {
        if (a is IPZFolder && b is IPZFile) return -1;
        if (a is IPZFile && b is IPZFolder) return 1;

        return NaturalComparer.Instance.Compare(a?.Name ?? "", b?.Name ?? "");
    }
}
