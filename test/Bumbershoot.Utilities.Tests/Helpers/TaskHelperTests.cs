using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        var invoking = fromResult.Invoking(x => x.OrFail());
        // assert
        await invoking.Should().ThrowAsync<Exception>().WithMessage("Failed to find string.");
    }

    [Test]
    public async Task GivenTaskCompleted_WhenContinueWithNoWaitCalled_ShouldInvokeAction()
    {
        // Arrange
        var result = 10;
        var task = Task.FromResult(result);
        var actionInvoked = false;

        // Act
        task.ContinueWithNoWait(_ => { actionInvoked = true; });
        // Assert
        var waitForAsync = await actionInvoked.WaitForAsync(_ => actionInvoked);
        actionInvoked.Should().BeTrue();
    }

    [Test]
    public async Task GivenTaskWithException_WhenContinueWithAndLogErrorCalled_ShouldLogError()
    {
        // Arrange
        var errorLogged = false;

        // Act
        var taskWithError = Task.FromException(new Exception("Test exception"));
        taskWithError.ContinueWithAndLogError((message, exception) =>
        {
            errorLogged = true;
            Console.WriteLine($"Error message: {message}");
            Console.WriteLine($"Exception: {exception}");
        });

        // Simulate async operation
        await errorLogged.WaitForAsync(_ => errorLogged);

        // Assert
        errorLogged.Should().BeTrue();
    }

    [Test]
    public async Task GivenTaskCompletedWithoutException_WhenContinueWithAndLogErrorCalled_ShouldNotLogError()
    {
        // Arrange
        var errorLogged = false;

        // Act
        var taskWithoutError = Task.CompletedTask;
        taskWithoutError.ContinueWithAndLogError((message, exception) =>
        {
            errorLogged = true;
            Console.WriteLine($"Error message: {message}");
            Console.WriteLine($"Exception: {exception}");
        });

        // Simulate async operation
        await errorLogged.WaitForAsync(_ => errorLogged, 10);
        // Assert
        errorLogged.Should().BeFalse();
    }

    [Test]
    public async Task GivenAListOfTask_ShouldIterateOverTasksButLimitTheConcurrency()
    {
        // arrange
        var stopwatch = Stopwatch.StartNew();
        var expected = 10;
        var millisecondsDelay = 100;
        var ran = 0;
        var concurrentRequests = 5;
        // action
        await Enumerable.Range(1, expected)
            .Select(async x =>
            {
                ran++;
                // Console.Out.WriteLine($"Before {x}");
                await Task.Delay(millisecondsDelay);
                // Console.Out.WriteLine($"After {x}");
                return Thread.CurrentThread.ManagedThreadId;
            }).WhenAllLimited(concurrentRequests);

        // assert
        stopwatch.Elapsed.TotalMilliseconds
            .Should().BeLessThan(millisecondsDelay * expected / 2d);
        ran.Should().Be(expected);
    }

    [Test]
    public async Task GivenTasksWithExceptions_WhenWhenAllLimitedCalled_ShouldThrowAggregateException()
    {
        // Arrange
        IEnumerable<Task<int>> tasks = new List<Task<int>>
        {
            Task.FromException<int>(new NotFiniteNumberException("Error 1")),
            Task.FromException<int>(new Exception("Error 2")),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromException<int>(new Exception("Error 3")),
        };
        var concurrentRequests = 3;
        // Act
        var invoking = tasks.Invoking(x => x.WhenAllLimited(concurrentRequests));
        // Assert
        await invoking.Should().ThrowAsync<AggregateException>()
            .WithInnerException(typeof(NotFiniteNumberException)).WithMessage("Error 1");
    }

    [Test]
    public async Task GivenTasksWithResults_WhenWhenAllLimitedCalled_ShouldReturnAllResults()
    {
        // Arrange
        var tasks = Enumerable.Range(1, 5)
            .Select(i => Task.FromResult(i));
        var concurrentRequests = 3;

        // Act
        var results = await tasks.WhenAllLimited(concurrentRequests);

        // Assert
        results.Should().BeEquivalentTo(new List<int> { 1, 2, 3, 4, 5 });
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

        var x = () => TaskHelper.Retry<ArgumentException>(Action, retries, 1);
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

        var tryAsync = () => TaskHelper.RetryAsync<ArgumentException>(Action, retries, 1).Wait();
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

        var tryAsync = () =>
            TaskHelper.RetryAsync<ArgumentException>(Call, retries, 1, (_, i) => backoff.Add(i)).Wait();
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

        void CallBack(ArgumentException _, int i) =>
            Console.WriteLine($"WARN: Failed with '{_.Message}', will retry in {i}ms.");

        try
        {
            await TaskHelper.RetryAsync<ArgumentException>(CallToRetry, retries, retryDelay, CallBack);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
        // assert
    }
}