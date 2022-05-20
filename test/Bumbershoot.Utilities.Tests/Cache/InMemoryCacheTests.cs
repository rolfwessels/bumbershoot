using System;
using Bumbershoot.Utilities.Cache;
using FluentAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Cache
{
    [TestFixture]
    public class InMemoryCacheTests
    {
        private ISimpleObjectCache _inMemoryCache;

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
        public void Get_WhenCacheDoesExistShoudOnlyDoItOnce_ShouldNotGetValue()
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