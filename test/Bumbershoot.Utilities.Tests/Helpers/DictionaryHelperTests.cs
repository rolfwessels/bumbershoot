using System.Collections.Generic;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class DictionaryHelperTests
    {
        [Test]
        public void GetOrAdd_GivenNewValue_ShouldOnlyAddAndReturn()
        {
            // arrange
            var dictionary = new Dictionary<string, int>();
            // action
            var result = dictionary.GetOrAdd("test", () => 1);
            // assert
            result.Should().Be(1);
            dictionary.Should().ContainKey("test").And.ContainValue(1);
        }

        [Test]
        public void GetOrAdd_GivenExistingValue_ShouldOnlyReturnValue()
        {
            // arrange
            var dictionary = new Dictionary<string, int> { { "test", 0 } };
            // action
            var result = dictionary.GetOrAdd("test", () => 1);
            // assert
            result.Should().Be(0);
            dictionary.Should().ContainKey("test").And.ContainValue(0);
        }
    }
}