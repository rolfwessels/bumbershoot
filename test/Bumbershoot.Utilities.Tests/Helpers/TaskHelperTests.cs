using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers;

public class TaskHelperTests
{
    [Test]
    public async Task OrFail_GivenString_ShouldReturnString()
    {
        // arrange
        var fromResult = Task.FromResult("result");
        // action
        var result = await fromResult.OrFail();
        // assert
        result.Should().Be("result");
    }

    [Test]
    public async Task OrFail_GivenNull_ShouldThrow()
    {
        // arrange
        var fromResult = Task.FromResult<string>(null);
        // action
        var invoking = fromResult.Invoking(x=>x.OrFail());
        // assert
        await invoking.Should().ThrowAsync<Exception>().WithMessage("Failed to find string.") ;
    }


}