using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void SampleSettings_GivenValidSettings_ShouldAlwaysReturnDefaultValue()
        {
            // arrange
            TestSettings.Initialize(new ConfigurationManager().AddInMemoryCollection(new Dictionary<string, string>()
            {
            }).Build());
            var testSettings = TestSettings.Instance;
            // action
            var value = testSettings.SampleSettings;
            // assert
            value.Should().Be("Bumbershoot-Sample");
        }


        [Test]
        public void SampleSettings_GivenInitializedValue_ShouldUseConfigurationValue()
        {
            // arrange
            var testSettings = Setup(new Dictionary<string, string>()
            {
                { "SampleSettings", "Memory" }
            });
            // action
            var value = testSettings.SampleSettings;
            // assert
            value.Should().Be("Memory");
        }


        [Test]
        public void BoolValue_GivenInitializedValue_ShouldUseConfigurationValue()
        {
            // arrange
            var testSettings = Setup(new Dictionary<string, string>()
            {
                { "BoolValue", "true" }
            });
            // action
            var value = testSettings.BoolValue;
            // assert
            value.Should().Be(true);
        }


        [Test]
        public void IntValue_GivenInitializedValue_ShouldUseConfigurationValue()
        {
            // arrange
            var testSettings = Setup(new Dictionary<string, string>()
            {
                { "IntValue", "12" }
            });
            // action
            var value = testSettings.IntValue;
            // assert
            value.Should().Be(12);
        }

        private static TestSettings Setup(Dictionary<string, string> initialData)
        {
            var testSettings = new TestSettings(new ConfigurationManager().AddInMemoryCollection(initialData).Build());
            return testSettings;
        }

        public class TestSettings : BaseSettings
        {
            private static Lazy<TestSettings> _instance = new(() => new TestSettings(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true).Build()));

            public TestSettings(IConfiguration configuration) : base(configuration, null)
            {
            }

            #region singleton

            public static TestSettings Instance => _instance.Value;

            #endregion

            public string SampleSettings => ReadConfigValue("SampleSettings", "Bumbershoot-Sample");
            public bool BoolValue => ReadConfigValue("BoolValue", false);
            public int IntValue => ReadConfigValue("IntValue", 1);

            public static void Initialize(IConfiguration configuration)
            {
                _instance = new Lazy<TestSettings>(() => new TestSettings(configuration));
            }
        }
    }

    
}