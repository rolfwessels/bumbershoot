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
        ok.Then(s => { found = s; }).Else(x => throw x);
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
        ok.Then(s => { found = s; }).Else(x => ex = x);
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
        var asSuccessValue = ok.Then(s => { })
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

    [Test]
    public void SampleClass_WhenCallingOkString_ShouldReturnResult()
    {
        // arrange
        var sample = new SampleClass();
        // action
        var result = sample.GetString();
        // assert
        result.IsSuccess.Should().BeTrue();
        result.AsSuccess?.Value.Should().Be("Hello, World!");
    }

    [Test]
    public void SampleClass_WhenCallingGetStringImplicit_ShouldReturnResult()
    {
        // arrange
        var sample = new SampleClass();
        // action
        var result = sample.GetStringImplicit();
        var intResult = sample.GetIntImplicit();
        // assert
        result.IsSuccess.Should().BeTrue();
        result.AsSuccess?.Value.Should().Be("Im implicit");
        intResult.AsSuccess?.Value.Should().Be(4);
    }


    [Test]
    public void SampleClass_WhenCallingImplicitException_ShouldReturnException()
    {
        // arrange
        var sample = new SampleClass();
        // action
        var result = sample.ImplicitException();
        // assert
        result.IsFailed.Should().BeTrue();
        result.FailedException!.Message.Should().Be("Implicit exception");
        result.FailedException.HResult.Should().Be(1);
    }


    public class SampleClass
    {
        public Output<string> GetString()
        {
            return Output.Ok("Hello, World!");
        }

        public Output<string> GetStringImplicit()
        {
            return "Im implicit";
        }

        public Output<int> GetIntImplicit()
        {
            return 4;
        }

        public Output<int> ImplicitException()
        {
            return new Exception("Implicit exception")
            {
                HResult = 1
            };
        }
    }
}