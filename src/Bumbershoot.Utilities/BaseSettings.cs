using System;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities
{
    public class BaseSettings
    {
        private readonly string _configGroup;
        private readonly IConfiguration _configuration;

        public BaseSettings(IConfiguration configuration, string configGroup)
        {
            _configuration = configuration;
            _configGroup = configGroup;
        }
        
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
            var section = string.IsNullOrWhiteSpace(_configGroup)
                ? _configuration
                : _configuration.GetSection(_configGroup);
            var value = section[key];
            return value ?? defaultValue;
        }

        protected void WriteConfigValue(string key, string value)
        {
            var section = string.IsNullOrWhiteSpace(_configGroup)
                ? _configuration
                : _configuration.GetSection(_configGroup);
            section[key] = value;
        }
    }
}