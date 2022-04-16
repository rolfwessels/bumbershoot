using System.Collections.Generic;
using Bumbershoot.Utilities.Encryption;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests;

public class BaseSettingsWithEncryptionTests
{
    private SettingsBTests _settings;

    [Test]
    public void method_GiventestingFor_Shouldresult()
    {
        // arrange
        Setup();
        // action
        _settings.Encrypted.Should().Be("hello");
        // assert
    }

    private void Setup()
    {
        var value = "enc";
        var encrypt = BaseSettingsWithEncryption.Prefix + new SimpleAes().Encrypt(value, "hello");
        var keyValuePairs = new Dictionary<string, string>()
        {
            { "key", value },
            { "Encrypted", encrypt }
        };
        var addInMemoryCollection = new ConfigurationManager().AddInMemoryCollection(keyValuePairs).Build();
        _settings = new SettingsBTests(addInMemoryCollection);
    }

    internal class SettingsBTests : BaseSettingsWithEncryption
    {
        public SettingsBTests(IConfiguration configuration)
            : base(configuration, "", "key")
        {
        }

        public string Encrypted => ReadConfigValue("Encrypted", "???");
    }
}