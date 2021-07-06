using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void MongoDatabase_GivenValidSettings_ShouldAlwaysReturnSetting()
        {
            // arrange
            // action
            Settings.Instance.MongoDatabase.Should().NotBeEmpty();
            // assert
        }
    }
}