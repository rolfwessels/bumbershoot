using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Cache;
using NUnit.Framework;

namespace Bumbershoot.Utilities.Tests.Cache;

public class FileCacheTests
{
    [Test]
    public void GetOrSet_GivenGivenRequest_ShouldTakeTheAsyncOverTheStandard()
    {
        // arrange
        var fileCache = new FileCache(TimeSpan.FromSeconds(10), "tst");
        // action
        fileCache.Reset();
        var value = fileCache.GetOrSet("acasd", () => Task.FromResult("asd"));
        // assert
    }
}