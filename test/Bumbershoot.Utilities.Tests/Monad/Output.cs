using System;
using System.Globalization;
using Bumbershoot.Utilities.Monad;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Monad;

public class OutputTests
{
    [Test]
    public void Output_GivenSuccess_ShouldBeAbleToGetValue()
    {
        // arrange
        var ok = Output.Ok("nice");
        
        // action
        Output<string>.Success found = null!;
        if (ok is Output<string>.Success result)
        {
            found = result;
        }
        // assert
        found.Value.Should().Be("nice");
        ok.IsSuccess.Should().Be(true);
        ok.IsFailed.Should().Be(false);
    }

    [Test]
    public void ThenElse_GivenSuccess_ShouldBeAbleToGetValue()
    {
        // arrange
        var ok = Output.Ok("nice");

        // action
        var found = "";
        ok.Then(s =>
        {
            found = s;
        }).Else(x=> throw x);
        // assert
        found.Should().Be("nice");
    }

    [Test]
    public void ThenElse_GivenFailure_ShouldCatchException()
    {
        // arrange
        var ok = Output.Failure<string>("failed");

        // action
        var found = "";
        Exception ex = null!;
        ok.Then(s =>
        {
            found = s;
        }).Else(x => ex = x);
        // assert
        found.Should().Be("");
        ex.Message.Should().Be("failed");
    }


    [Test]
    public void ThenElseMap_GivenSuccess_ShouldBeAbleToMap()
    {
        // arrange
        var ok = Output.Ok("1");
        // action
        var asSuccessValue = ok.Then(s =>
            {
            
            })
            .ThenMap(Convert.ToInt32)
            .Else(x => throw x)
            .AsSuccess?.Value;
        // assert
        asSuccessValue.Should().Be(1);
    }

    [Test]
    public void ThenElseMap_GivenFailure_ShouldHaveException()
    {
        // arrange
        var ok = Output.Ok("one");
        // action
        var exception = ok
            .ThenMap(Convert.ToInt32)
            .FailedException;
        // assert
        exception.Should().BeOfType<FormatException>();
    }
}