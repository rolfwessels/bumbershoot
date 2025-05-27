using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using AwesomeAssertions;
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
        stopwatch.ElapsedMilliseconds.Should().BeLessThanOrEqualTo(50 * 3); //allow slowness
    }

    [Test]
    public async Task WaitForAsync_GivenValidValue_ShouldReturnQuickSticks()
    {
        // action
        var stopwatch = new Stopwatch().With(x => x.Start());
        await "test".WaitForAsync(x => x == "test", 50);
        // assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Test]
    public async Task WaitForAsync_GivenClassWithAsyncMethod_ShouldCallMethodUntilValueResult()
    {
        // action
        var sampleWithAsyncMethod = new SampleWithAsyncMethod();
        var expected = "test-2";
        var waitForAsync = await sampleWithAsyncMethod.WaitForAsync(x => x.GetValueAsync(), x => x == expected, 50);
        // assert
        waitForAsync.Should().Be(expected);
    }

    [Test]
    public void WaitFor_GivenValidValue_ShouldReturnQuickSticks()
    {
        // action
        var stopwatch = new Stopwatch().With(x => x.Start());
        "test".WaitFor(x => x == "test", 50);
        // assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }


    [TestCase(200.2, "200ms")]
    [TestCase(1201, "1.2s")]
    [TestCase(65201, "1.1m")]
    [TestCase(35365201, "9.8h")]
    [TestCase(353652011, "4.1d")]
    public void ShortTime_GivenValidValue_ShouldReturnShortString(double milliSeconds, string expected)
    {
        // action
        var shortTime = TimeSpan.FromMilliseconds(milliSeconds).ShortTime();
        // assert
        shortTime.Should().Be(expected);
    }

    public class SampleWithAsyncMethod()
    {
        public int called = 1;

        public async Task<string> GetValueAsync()
        {
            await Task.Delay(10);
            return "test-" + called++;
        }
    }
}