using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bumbershoot.Utilities.Helpers;

namespace Bumbershoot.Utilities.Encryption;

// from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-6.0

public class SimpleAes : ISimpleEncryption
{
    public string Encrypt(string key, string decryptedValue)
    {
        using var aesAlg = Aes.Create();
        var rgbKey = GetValidKey(key);
        var iv = aesAlg.IV;
        var encrypt = aesAlg.CreateEncryptor(rgbKey, iv);
        
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encrypt, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(decryptedValue);
        }
        return iv.Combine(msEncrypt.ToArray()).Base64String();
    }

    private static byte[] GetValidKey(string key)
    {
        var buffer = key.ToBytes();
        if (buffer.Length is 16 or 32) return buffer;
        return MD5.Create().ComputeHash(buffer);
    }

    public string Decrypt(string key, string encryptedValue)
    {
        using var aesAlg = Aes.Create();
        var rgbKey = GetValidKey(key);
        var (iv, data) = Convert.FromBase64String(encryptedValue).Split(16);
        using var decryptor = aesAlg.CreateDecryptor(rgbKey, iv);
        using var msDecrypt = new MemoryStream(data);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        return csDecrypt.ReadToString();
    }
}

public interface ISimpleEncryption
{
    string Encrypt(string key, string decryptedValue);
    string Decrypt(string key, string encryptedValue);
}
