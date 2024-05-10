using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Helpers;

public static class TaskHelper
{
    public static void ContinueWithNoWait<TType>(this Task<TType> updateAllReferences,
        Action<Task<TType>> logUpdate)
    {
        var allReferences = updateAllReferences;
        allReferences.ConfigureAwait(false);
        allReferences.ContinueWith(logUpdate);
    }

    public static void ContinueWithAndLogError(this Task sendAsync, Action<string, Exception> log = null)
    {
        sendAsync.ContinueWith(x =>
        {
            if (x.Exception != null) log?.Invoke($"Failed to run async method:{x.Exception.Message}", x.Exception);
        });
    }

    public static async Task<T> OrFail<T>(this Task<T?> lookup)
    {
        var found = await lookup;
        if (found == null) throw new Exception($"Failed to find {typeof(T).Name}.");
        return found;
    }

    public static async Task<List<T>> WhenAllLimited<T>(this IEnumerable<Task<T>> tasks, int concurrentRequests)
    {
        var results = new ConcurrentBag<T>();
        var semaphoreSlim = new SemaphoreSlim(concurrentRequests, concurrentRequests);
        var running = new List<Task>();
        foreach (var task in tasks)
        {
            await semaphoreSlim.WaitAsync();
            var continuedTask = task.ContinueWith(x =>
            {
                var release = semaphoreSlim.Release();
                if (x.Exception != null) throw x.Exception;
                results.Add(x.Result);
                return release;
            });
            running.Add(continuedTask);
        }

        await Task.WhenAll(running);
        return results.ToList();
    }
}