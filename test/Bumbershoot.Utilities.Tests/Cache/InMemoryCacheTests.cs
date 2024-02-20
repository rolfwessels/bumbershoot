using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Cache;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Cache
{
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
            var key = "value";
            ISimpleObjectCache cache = type switch
            {
                "InMemoryCache" => new InMemoryCache(defaultCacheTime),
                "FileCache" => new FileCache(defaultCacheTime,"tst"),
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
            await Task.WhenAll(result1,resultAgain, result2);
            var getResultBefore = cache.GetAsync<string>(key);
            await Task.Delay(defaultCacheTime+defaultCacheTime);
            var getResult = cache.Get<string>(key);
            await Task.Delay(defaultCacheTime);
            var reset = cache.Reset(key);
            var result3 = cache.GetOrSet(key, () => "two");
            
            // assert
            called.Should().Be(1);
            getResultBefore.Result.Should().Be("one");
            resultAgain.Result.Should().Be("one");
            getResult.Should().Be(null);
            reset.Should().BeFalse();
            result1.Result.Should().Be("one");
            result1.Result.Should().Be("one");
            result2.Result.Should().Be("one"); 
            result3.Should().Be("two"); 
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
            _inMemoryCache.GetOrSet("value", () => { return "newValue"; });
            // action
            var result = _inMemoryCache.Get<string>("value");
            // assert
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
    }

   
}