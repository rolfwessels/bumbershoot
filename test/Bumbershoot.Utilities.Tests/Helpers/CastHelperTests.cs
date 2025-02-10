using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers;

[TestFixture]
public class CastHelperTests
{
    [Test]
    public void Dump_GivenStringAndName_ShouldOutputTheValueAndReturnOriginal()
    {
        // arrange
        var one = new One { Value1 = "Value11", Value2 = "Value21" };
        // action
        var two = one.DynamicCastTo<Two>();
        // assert
        two.GetType().Name.Should().Be("Two");
        two.Value1.Should().Be("Value11");
        two.Value2.Should().Be("Value21");
    }

    internal class One
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }

    internal class Two
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }
}