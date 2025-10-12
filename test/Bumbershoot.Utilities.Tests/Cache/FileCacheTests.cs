using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Cache;
using AwesomeAssertions;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Cache;

[TestFixture]
public class FileCacheTests
{
    private FileCache _fileCache;

    #region Setup/Teardown

    public void Setup()
    {
        _fileCache = new FileCache(TimeSpan.FromSeconds(10), "test_cache");
        _fileCache.Reset(); // Clear any existing cache
    }

    [TearDown]
    public void TearDown()
    {
        _fileCache?.Reset(); // Clean up after tests
    }

    #endregion

    [Test]
    public void FileCache_WhenCreatedWithCustomPrefix_ShouldUseCustomDirectory()
    {
        // arrange
        Setup();
        var customPrefix = "custom_test_cache";
        var cache = new FileCache(TimeSpan.FromMinutes(5), customPrefix);

        try
        {
            // action
            cache.Set("test_key", "test_value");
            var result = cache.Get<string>("test_key");

            // assert
            result.Should().Be("test_value");
        }
        finally
        {
            cache.Reset();
        }
    }

    [Test]
    public void ComplexObject_WhenStoringAndRetrieving_ShouldSerializeCorrectly()
    {
        // arrange
        Setup();
        var complexObject = CreateTestObject();

        // action
        _fileCache.Set("complex_key", complexObject);
        var retrieved = _fileCache.Get<TestObject>("complex_key");

        // assert
        retrieved.Should().NotBe(null);
        retrieved.Id.Should().Be(complexObject.Id);
        retrieved.Name.Should().Be(complexObject.Name);
        retrieved.CreatedAt.Should().Be(complexObject.CreatedAt);
        retrieved.IsActive.Should().Be(complexObject.IsActive);
    }

    [Test]
    public void FileCache_WhenApplicationRestarts_ShouldPersistData()
    {
        // arrange
        Setup();
        var testValue = "persistent_value";
        _fileCache.Set("persist_key", testValue);

        // action - simulate application restart by creating new cache instance
        var newCacheInstance = new FileCache(TimeSpan.FromMinutes(10), "test_cache");
        var result = newCacheInstance.Get<string>("persist_key");

        // assert
        result.Should().Be(testValue);

        // cleanup
        newCacheInstance.Reset();
    }

    [Test]
    public async Task ConcurrentAccess_WhenMultipleThreadsAccess_ShouldBeThreadSafe()
    {
        // arrange
        Setup();
        var factoryCallCount = 0;
        var tasks = new Task<string>[10];

        // action
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = _fileCache.GetOrSetAsync("concurrent_key", async () =>
            {
                factoryCallCount++;
                await Task.Delay(50);
                return "concurrent_value";
            });
        }

        var results = await Task.WhenAll(tasks);

        // assert
        factoryCallCount.Should().Be(1); // Factory should only be called once due to semaphore
        foreach (var result in results)
        {
            result.Should().Be("concurrent_value");
        }
    }


    [Test]
    public async Task GetStaleAsync_WhenNotExpired_ShouldReturnValue()
    {
        // arrange
        Setup();
        _fileCache.Set("fresh:key", "fresh-file");

        // act
        var stale = await _fileCache.GetStaleAsync<string>("fresh:key");
        var fresh = await _fileCache.GetAsync<string>("fresh:key");

        // assert
        stale.Should().Be("fresh-file");
        fresh.Should().Be("fresh-file");
    }

    private static TestObject CreateTestObject() => new()
    {
        Id = 123,
        Name = "Test Object",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    public class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}