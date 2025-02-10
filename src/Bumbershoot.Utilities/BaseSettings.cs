using System;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities;

public class BaseSettings(IConfiguration configuration, string configGroup)
{
    protected bool ReadConfigValue(string key, bool defaultValue)
    {
        return Convert.ToBoolean(ReadConfigValue(key, defaultValue.ToString()));
    }

    protected int ReadConfigValue(string key, int defaultValue)
    {
        return Convert.ToInt32(ReadConfigValue(key, defaultValue.ToString()));
    }

    protected virtual string ReadConfigValue(string key, string defaultValue)
    {
        var section = string.IsNullOrWhiteSpace(configGroup)
            ? configuration
            : configuration.GetSection(configGroup);
        var value = section[key];
        return value ?? defaultValue;
    }

    protected void WriteConfigValue(string key, string value)
    {
        var section = string.IsNullOrWhiteSpace(configGroup)
            ? configuration
            : configuration.GetSection(configGroup);
        section[key] = value;
    }
}