using PZPK.Core.Utility;
using System.Security.Cryptography;

namespace PZPK.Core.Crypto;

public class PZCrypto
{
    public static Aes CreateAes()
    {
        Aes cryptor = Aes.Create();
        cryptor.Mode = CipherMode.CBC;
        cryptor.Padding = PaddingMode.PKCS7;
        cryptor.KeySize = 256;
        cryptor.GenerateIV();

        return cryptor;
    }
    public static byte[] CreateKey(string password)
    {
        return HashHelper.Sha256(password);
    }
    public static void GenerateIV(Span<byte> data)
    {
        RandomNumberGenerator.Fill(data);
    }

    public static IPZCrypto Create(int version, byte[] key, int blockSize)
    {
        return version switch
        {
            1 or 2 => new PZCryptoV2(key),
            4 => new PZCryptoV4(key),
            11 or 12 or 20 => new PZCryptoV11(key, blockSize),
            _ => throw new Exceptions.FileVersionNotCompatiblityException(version)
        };
    }
}
