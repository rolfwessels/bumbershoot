using System;
using Bumbershoot.Utilities.Encryption;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities;

public class BaseSettingsWithEncryption : BaseSettings
{
    public static string Prefix => "EN|";
    private readonly string _key;
    private readonly ISimpleEncryption _encryption;

    public BaseSettingsWithEncryption(IConfiguration configuration, string configGroup, string keyGroup = "EncyptionKey", ISimpleEncryption? encryption = null) : base(configuration, configGroup)
    {
        _key = configuration[keyGroup] ?? throw new ArgumentException($"Config required the {keyGroup} to be set.");
        _encryption = encryption?? new SimpleAes();
    }

    #region Overrides of BaseSettings

    protected override string ReadConfigValue(string key, string defaultValue)
    {
        var readConfigValue = base.ReadConfigValue(key, defaultValue);
        if (readConfigValue.StartsWith(Prefix))
        {
            return _encryption.Decrypt(_key, readConfigValue.Substring(3, readConfigValue.Length - 3));
        }
        return readConfigValue;
    }

    #endregion

    
}