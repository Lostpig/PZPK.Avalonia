namespace PZPK.Core.Utility;

internal class Compatible
{

    // 1,2,4
    // 11,12
    // 20
    public static bool IsCompatibleVersion(int version)
    {
        return version switch
        {
            1 or 2 or 4 => true,
            11 => true,
            12 => true,
            Constants.Version => true,
            _ => false
        };
    }

    public static byte[] CreateKeyHash(byte[] key)
    {
        string hex = HashHelper.Sha256Hex(key);
        return HashHelper.Sha256(hex);
    }
}
