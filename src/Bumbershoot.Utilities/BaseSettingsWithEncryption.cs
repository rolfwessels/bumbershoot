using System;
using Bumbershoot.Utilities.Encryption;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities;

public class BaseSettingsWithEncryption(
    IConfiguration configuration,
    string configGroup,
    string keyGroup = "EncryptionKey",
    ISimpleEncryption? encryption = null,
    string prefix = "EN|")
    : BaseSettings(configuration, configGroup)
{
    private readonly Lazy<string> _key = new(() =>
        configuration[keyGroup] ?? throw new ArgumentException($"Config required the {keyGroup} to be set."));

    private readonly ISimpleEncryption _encryption = encryption ?? new SimpleAes();
    public string Prefix { get; } = prefix;

    public string GetEncryptedValue(string test)
    {
        return Prefix + _encryption.Encrypt(_key.Value, test);
    }


    protected override string ReadConfigValue(string key, string defaultValue)
    {
        var readConfigValue = base.ReadConfigValue(key, defaultValue);
        if (readConfigValue.StartsWith(Prefix))
        {
            return _encryption.Decrypt(_key.Value,
                readConfigValue.Substring(Prefix.Length, readConfigValue.Length - Prefix.Length));
        }

        return readConfigValue;
    }
}