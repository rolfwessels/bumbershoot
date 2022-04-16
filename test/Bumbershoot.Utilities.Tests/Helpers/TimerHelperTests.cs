using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Helpers;

public class TimerHelperTests
{
    [Test]
    public void WaitFor_Given100ms_ShouldNotWaitLongerThan100ms()
    {
        // action
        var stopwatch = new Stopwatch().With(x => x.Start());
        var waitFor = "test".WaitFor(x => x == "", 50);
        // assert
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(50);
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(50*3);//allow slowness
    }

    [Test]
    public void WaitFor_GivenValidValue_ShouldReturnQuickSticks()
    {
        // action
        var stopwatch = new Stopwatch().With(x=>x.Start());
        "test".WaitFor(x => x == "test", 50);
        // assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    
    [TestCase(200, "200ms")]
    [TestCase(1201, "1.2s")]
    [TestCase(65201, "1.1m")]
    [TestCase(35365201, "9.8h")]
    [TestCase(353652011, "4.1d")]
    public void ShortTime_GivenValidValue_ShouldReturnShortString(long milliSeconds,string expected)
    {
        // action
        var shortTime = TimeSpan.FromMilliseconds(milliSeconds).ShortTime();
        // assert
        shortTime.Should().Be(expected);
    }

    [Test]
    public void Retry_GivenValidValue_ShouldRetryXTimes()
    {
        var retries = 4;
        // action
        var counter = 0;


        void Action()  
        {
            counter++;
            throw new ArgumentException();
        }

        Action x = () => TimerHelper.Retry<ArgumentException>(Action,retries,1);
        // assert
        x.Should().Throw<ArgumentException>();
        counter.Should().Be(retries);
    }

    [Test]
    public void RetryTryAsync_GivenValidValue_ShouldRetryXTimes()
    {
        var retries = 4;
        // action
        var counter = 0;

        async Task Action()
        {
            counter++;
            await Task.Delay(1);
            throw new ArgumentException();
        }

        Action tryAsync = () => TimerHelper.RetryAsync<ArgumentException>(Action,retries,1).Wait();
        // assert
        tryAsync.Should().Throw<ArgumentException>();
        counter.Should().Be(retries);
    }

    [Test]
    public void RetryTryAsync_GivenValidValue_ShouldBackOff()
    {
        var retries = 4;
        var backoff = new List<int>();
        // action
        async Task Call()
        {
            await Task.Delay(1);
            throw new ArgumentException();
        }

        Action tryAsync = ()=> TimerHelper.RetryAsync<ArgumentException>(Call,retries,1,(_, i) => backoff.Add(i)).Wait();
        // assert
        tryAsync.Should().Throw<ArgumentException>();
        backoff.Should().Contain(1);
        backoff.Should().Contain(2);
        backoff.Should().Contain(4);

    }

    [Test]
    public async Task RetryTryAsync_GivenSample_ShouldOutput()
    {
        var retries = 4;
        var retryDelay = 10;
        // action
        async Task CallToRetry()
        {
            await Task.Delay(1);
            throw new ArgumentException();
        }
        void CallBack(ArgumentException _, int i) => Console.WriteLine($"WARN: Failed with '{_.Message}', will retry in {i}ms.");
        try
        {   
            await TimerHelper.RetryAsync<ArgumentException>(CallToRetry, retries, retryDelay, CallBack);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
        // assert
        

    }

}