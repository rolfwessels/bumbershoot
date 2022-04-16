using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Bumbershoot.Utilities.Helpers;

namespace Bumbershoot.Utilities.Encryption;

// from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-6.0
public class SimpleAes
{
    public string Encrypt(string key, string decryptedValue)
    {
        using Aes aesAlg = Aes.Create();
        var rgbKey = getValidKey(key);
        var iv = "validKey12332111".ToBytes() ?? aesAlg.IV;
        var encrypt = aesAlg.CreateEncryptor(rgbKey, iv);
        
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encrypt, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(decryptedValue);
        }

        iv.Base64String().Dump("iv"+ iv.Length);
        rgbKey.Base64String().Dump("rgbKey" + rgbKey.Length);
        return msEncrypt.ToArray().Base64String();
    }

    private static byte[] getValidKey(string key)
    {   
        return MD5.Create().ComputeHash(key.ToBytes()); 
    }

    public string Decrypt(string key, string encryptedValue)
    {
        using (Aes aesAlg = Aes.Create())
        {
            var rgbKey = getValidKey(key);
            
            var (iv1, data) = encryptedValue.ToBytes().Split(16);
            var iv = "validKey12332111".ToBytes() ?? aesAlg.IV;
            iv.Base64String().Dump("iv");
            rgbKey.Base64String().Dump("rgbKey" + rgbKey.Length);
    

            
            using var decryptor = aesAlg.CreateDecryptor(rgbKey, iv);
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedValue));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
            
        }
    }
}