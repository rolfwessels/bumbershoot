using System;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers;

[TestFixture]
public class EmbeddedResourceHelperTests
{
    [Test]
    public void ReadResource_GivenInvalidResource_ShouldThrowException()
    {
        // arrange
        const string path = "Bumbershoot.Utilities.Tests.Resources.t1.txt";
        // action
        var testCall = () => { EmbeddedResourceHelper.ReadResource(path, typeof(EmbeddedResourceHelperTests)); };
        // assert
        testCall.Should().Throw<ArgumentException>().WithMessage(
            "Bumbershoot.Utilities.Tests.Resources.t1.txt resource does not exist in Bumbershoot.Utilities.Tests assembly. Did you mean [Bumbershoot.Utilities.Tests.Resources.t.txt]?");
    }

    [Test]
    public void ReadResource_GivenValidResource_ShouldReturnString()
    {
        // arrange
        const string path = "Bumbershoot.Utilities.Tests.Resources.t.txt";
        // action
        var readResource = EmbeddedResourceHelper.ReadResource(path, typeof(EmbeddedResourceHelperTests));
        // assert
        readResource.Should().Be("sample");
    }
}