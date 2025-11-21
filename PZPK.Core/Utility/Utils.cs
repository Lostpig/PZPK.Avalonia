namespace PZPK.Core.Utility;

internal static class Utils
{
    public static bool CompareBytes(ReadOnlySpan<byte> bytes1, ReadOnlySpan<byte> bytes2)
    {
        if (bytes1.Length != bytes2.Length) return false;
        for (int i = 0; i < bytes1.Length; i++)
        {
            if (bytes1[i] != bytes2[i]) return false;
        }
        return true;
    }
}
