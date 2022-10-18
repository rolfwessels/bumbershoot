using System.Collections.Generic;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers
{
    [TestFixture]
    public class EnumerableHelperTests
    {
        [Test]
        public void ForEach_GivenEnum_ShouldRunForeachOverAll()
        {
            // arrange
            var ints = new[] { 1, 23, 4, 5, 7, 8, 5678, 809, 790 };
            var counter = 0;
            // action
            var stringJoin = ints.ForEach(i => counter++);
            // assert
            ints.Count().Should().Be(counter);
        }

        [Test]
        public void StringJoin_WhenCalledWithArrayOfStrings_ShouldJoinValues()
        {
            // arrange
            var values = new[] { "1", "2" };
            // action
            var stringJoin = values.StringJoin();
            // assert
            stringJoin.Should().Be("1, 2");
        }

        [Test]
        public void StringJoin_WhenCalledWithDifferenceSeparator_ShouldUseSeparator()
        {
            // arrange
            var values = new[] { "1", "2" };
            // action
            var stringJoin = values.StringJoin("-");
            // assert
            stringJoin.Should().Be("1-2");
        }

        [Test]
        public void StringJoin_WhenCalledWithOneNull_ShouldReturnNull()
        {
            // arrange
            var values = null as IEnumerable<object>;
            // action
            var stringJoin = values.StringJoin("-");
            // assert
            stringJoin.Should().Be("");
        }

        [Test]
        public void StringJoin_WhenCalledWithOneValue_ShouldDisplayOnlyValue()
        {
            // arrange
            var values = new[] { "1" };
            // action
            var stringJoin = values.StringJoin("-");
            // assert
            stringJoin.Should().Be("1");
        }


        [Test]
        public void LookupValidValue_GivenValue_ShouldMatchValue()
        {
            // arrange
            var values = new[] { "Guest", "Smesht", "Lest" };
            // action
            var found = values.LookupValidValue("guest");
            // assert
            found.Should().Be("Guest");
        }

        [Test]
        public void LookupValidValue_GivenValueWithDifferentCase_ShouldMatchValue()
        {
            // arrange
            var values = new[] { "Guest", "Smesht", "Lest" };
            // action
            var found = values.LookupValidValue("smesht");
            // assert
            found.Should().Be("Smesht");
        }


        [Test]
        public void LookupValidValue_GivenInvalidValue_ShouldReturnNull()
        {
            // arrange
            var values = new[] { "Guest", "Smesht", "Lest" };
            // action
            var found = values.LookupValidValue("Best");
            // assert
            found.Should().Be(null);
        }
    }
}