using System;
using Bumbershoot.Utilities.Encryption;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities;

public class BaseSettingsWithEncryption : BaseSettings
{
    private readonly Lazy<string> _key;
    private readonly ISimpleEncryption _encryption;
    public string Prefix { get; }

    public BaseSettingsWithEncryption(IConfiguration configuration, string configGroup, string keyGroup = "EncryptionKey", ISimpleEncryption? encryption = null , string prefix = "EN|") : base(configuration, configGroup)
    {
        Prefix = prefix;
        _key = new Lazy<string>(() =>
            configuration[keyGroup] ?? throw new ArgumentException($"Config required the {keyGroup} to be set."));
        
        _encryption = encryption?? new SimpleAes();
    }

    public string GetEncryptedValue(string test)
    {
        return Prefix+_encryption.Encrypt(_key.Value, test);
    }
    

    protected override string ReadConfigValue(string key, string defaultValue)
    {
        var readConfigValue = base.ReadConfigValue(key, defaultValue);
        if (readConfigValue.StartsWith(Prefix))
        {
            return _encryption.Decrypt(_key.Value, readConfigValue.Substring(Prefix.Length, readConfigValue.Length - Prefix.Length));
        }
        return readConfigValue;
    }
    
    
}