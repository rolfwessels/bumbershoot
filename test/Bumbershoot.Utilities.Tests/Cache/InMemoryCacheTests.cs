using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Cache;
using AwesomeAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Cache;

[TestFixture]
public class InMemoryCacheTests
{
    private InMemoryCache _inMemoryCache;

    #region Setup/Teardown

    public void Setup()
    {
        _inMemoryCache = new InMemoryCache(TimeSpan.FromSeconds(1));
    }

    [TearDown]
    public void TearDown()
    {
    }

    #endregion

    [Test]
    [TestCase("InMemoryCache")]
    [TestCase("FileCache")]
    public async Task GetOrSet_WhenWhenTimesOut_ShouldUseNewValue(string type)
    {
        // arrange
        var defaultCacheTime = TimeSpan.FromMilliseconds(100);
        var key = "value" + DateTime.Now;
        ISimpleObjectCacheASync cache = type switch
        {
            "InMemoryCache" => new InMemoryCache(defaultCacheTime),
            "FileCache" => new FileCache(defaultCacheTime, "tst"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        var called = 0;
        var value = async () =>
        {
            called++;
            await Task.Delay(10);
            return "one";
        };
        // action
        cache.Reset();

        var result2 = cache.GetOrSetAsync(key, value);
        var resultAgain = cache.GetOrSetAsync(key, value);
        var result1 = cache.GetAndResetAsync(key, value);
        await Task.WhenAll(result1, resultAgain, result2);
        await Task.Delay(defaultCacheTime + defaultCacheTime);
        var cashSync = (ISimpleObjectCache)cache;
        var getResult = cashSync.Get<string>(key);
        await Task.Delay(defaultCacheTime);
        var reset = cache.Reset(key);
        var result3 = cashSync.GetOrSet(key, () => "two");

        // assert
        result1.Result.Should().Be("one");
        called.Should().Be(1);
        resultAgain.Result.Should().Be("one");
        getResult.Should().Be(null);
        reset.Should().BeFalse();
        result1.Result.Should().Be("one");
        result1.Result.Should().Be("one");
        result2.Result.Should().Be("one");
        result3.Should().Be("two");
    }

    [Test]
    [TestCase("InMemoryCache")]
    [TestCase("FileCache")]
    public async Task GetStaleAsync_WhenValueExpired_ShouldReturnStaleValue(string type)
    {
        // arrange
        var expiry = TimeSpan.FromMilliseconds(25);
        ISimpleObjectCacheASync cache = type == "InMemoryCache"
            ? new InMemoryCache(expiry)
            : new FileCache(expiry, "stale_unified");

        var orSetAsync = await cache.GetOrSetAsync("stale:key", () => Task.FromResult("stale-value"));
        var notStale = await cache.GetStaleAsync<string>("stale:key")!;
        await Task.Delay(60); // exceed expiry
        var stale = await cache.GetStaleAsync<string>("stale:key")!;

        // assert
        orSetAsync.Should().Be("stale-value");
        notStale.Should().Be("stale-value");
        stale.Should().Be("stale-value");
    }


    [Test]
    public void Get_WhenCacheDoesExist_ShouldNotGetValue()
    {
        // arrange
        Setup();
        _inMemoryCache.Set("value", "newValue");
        // action
        var result = _inMemoryCache.Get<string>("value");
        // assert
        result.Should().Be("newValue");
    }


    [Test]
    public void Get_WhenCacheDoesExistShouldOnlyDoItOnce_ShouldNotGetValue()
    {
        // arrange
        Setup();
        var value = "value";
        var orSet = _inMemoryCache.GetOrSet(value, () => { return "newValue"; });
        // action
        var result = _inMemoryCache.Get<string>(value);
        // assert
        orSet.Should().Be("newValue");
        result.Should().Be("newValue");
    }

    [Test]
    public void Get_WhenCacheDoesNotExist_ShouldReturnValue()
    {
        // arrange
        Setup();
        // action
        var result = _inMemoryCache.GetOrSet("value", () => { return "newValue"; });
        // assert
        result.Should().Be("newValue");
    }

    [Test]
    public void Get_WhenWithNoSet_ShouldJustRetrieveTheValue()
    {
        // arrange
        Setup();
        _inMemoryCache.Set("value", "newValue");
        // action
        var result = _inMemoryCache.Get<string>("value");
        // assert
        result.Should().Be("newValue");
    }

    [Test]
    public void GetOrSet_WhenWithNoSet_ShouldJustRetrieveTheValue()
    {
        // arrange
        Setup();
        // action
        var result1 = _inMemoryCache.GetOrSet("value", () => "one");
        var result2 = _inMemoryCache.GetOrSet("value", () => "two");
        // assert
        result1.Should().Be("one");
        result2.Should().Be("one"); // because value is already in the cache
    }


    [Test]
    public void Reset_WhenWithNoSet_ShouldJustRemoveOneValue()
    {
        // arrange
        Setup();
        // action
        var result1 = _inMemoryCache.GetOrSet("value", () => "one");
        _inMemoryCache.Reset("value");
        var result2 = _inMemoryCache.GetOrSet("value", () => "two");
        // assert
        result1.Should().Be("one");
        result2.Should().Be("two"); // because value is already in the cache
    }


    [Test]
    public void Reset_WhenCalledForAll_ShouldRemoveAllValues()
    {
        // arrange
        Setup();
        // action
        var result1 = _inMemoryCache.GetOrSet("value", () => "one");
        _inMemoryCache.Reset();
        var result2 = _inMemoryCache.GetOrSet("value", () => "two");
        // assert
        result1.Should().Be("one");
        result2.Should().Be("two"); // because value is already in the cache
    }

    [Test]
    [TestCase("InMemoryCache")]
    [TestCase("FileCache")]
    public async Task GetOrRefreshAsync_WhenValueIsFresh_ShouldReturnWithoutRefresh(string type)
    {
        // arrange
        var callCount = 0;
        var expiry = TimeSpan.FromMilliseconds(10);
        ISimpleObjectCacheASync cache = type == "InMemoryCache"
            ? new InMemoryCache(expiry)
            : new FileCache(expiry, "refresh_fresh");

        var factory = async () =>
        {
            callCount++;
            await Task.Delay(1);
            return $"value-{callCount}";
        };
        cache.Reset();

        var first = cache.GetOrRefreshAsync("fresh:key", factory);
        var second = cache.GetOrRefreshAsync("fresh:key", factory);
        await Task.WhenAll(second, first);

        await Task.Delay(50); // wait to ensure no background call
        var third = await cache.GetOrRefreshAsync("fresh:key", factory);
        var fourth = await cache.GetOrRefreshAsync("fresh:key", factory);

        // assert
        first.Result.Should().Be("value-1");
        second.Result.Should().Be("value-1");
        third.Should().Be("value-1");
        fourth.Should().Be("value-2");
        callCount.Should().Be(2); // factory should NOT be called again
    }
}