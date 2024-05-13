using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests;

public class ConfigurationFactoryTests
{
    [Test]
    public void GetEnvString_GivenDefault_ShouldReturnLocal()
    {
        // action
        var envString = ConfigurationFactory.GetEnvString();
        // assert
        envString.Should().Be("local");
    }

    [Test]
    public void InformationalVersion_GivenCallingAssembly_ShouldReturnVersion()
    {
        // action
        var informationalVersion =
            ConfigurationFactory.InformationalVersion(typeof(ConfigurationFactoryTests).Assembly);
        // assert
        informationalVersion.Should().Be("0.0.1-tests");
    }

    [Test]
    public void GivenJsonFile_ShouldLoadSettingFromJsonFile()
    {
        // arrange
        var configuration = ConfigurationFactory.Load();
        var testSettings = new TestSettings(configuration);
        // action
        var testSettingsJsonSetting = testSettings.JsonSetting;
        // assert
        testSettingsJsonSetting.Should().Be("yes");
    }

    [Test]
    public void GivenEnvironmentVariable_ShouldOverrideSetting()
    {
        // arrange
        Environment.SetEnvironmentVariable("EnvSetting", "yes");
        var configuration = ConfigurationFactory.Load();
        var testSettings = new TestSettings(configuration);
        // action
        var testSettingsJsonSetting = testSettings.EnvSetting;
        // assert
        testSettingsJsonSetting.Should().Be("yes");
    }

    public class TestSettings : BaseSettings
    {
        public TestSettings(IConfiguration configuration) : base(configuration, "")
        {
        }

        public string JsonSetting => ReadConfigValue("JsonSetting", "Nope");
        public string EnvSetting => ReadConfigValue("EnvSetting", "Nope");
    }
}