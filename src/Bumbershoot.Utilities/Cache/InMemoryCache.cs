using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;

namespace Bumbershoot.Utilities.Cache;

public class InMemoryCache : ISimpleObjectCache, ISimpleObjectCacheASync
{
    private readonly TimeSpan _defaultCacheTime;
    private readonly ConcurrentDictionary<string, CacheHolder> _objectCache;
    private DateTime _nextExpiry;

    public InMemoryCache(TimeSpan defaultCacheTime)
    {
        _defaultCacheTime = defaultCacheTime;
        _objectCache = new ConcurrentDictionary<string, CacheHolder>();
        _nextExpiry = DateTime.Now.Add(_defaultCacheTime);
    }

    public TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class
    {
        if (_objectCache.TryGetValue(key, out var values))
        {
            if (!values.IsExpired)
            {
                var asValue = values.AsValue<TValue>();
                if (asValue != null)
                    return asValue;
            }
            else
            {
                StartCleanup();
            }
        }

        var andReset = Set(key, getValue());

        return andReset;
    }

    public Task<TValue> GetAndResetAsync<TValue>(string key, Func<Task<TValue>> getValue)
    {
        return GetAndReset(key, getValue);
    }


    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValue)
    {
        var cacheHolder =
            _objectCache.GetOrAdd(key, _ => new CacheHolder(getValue(), DateTime.Now.Add(_defaultCacheTime)));
        if (cacheHolder.IsExpired)
            StartCleanup();
        else
            return cacheHolder.AsValue<Task<T>>()!;
        return Set(key, getValue());
    }


    public T GetOrSet<T>(string key, Func<T> func) where T : class
    {
        return GetOrSetAsync(key, () => Task.FromResult(func())).GetAwaiter().GetResult();
    }

    public Task<TValue> SetAsync<TValue>(string key, Task<TValue> value)
    {
        Debug.Assert(value != null, nameof(value) + " != null");
        var cacheHolder = new CacheHolder(value, DateTime.Now.Add(_defaultCacheTime));
        _objectCache.AddOrUpdate(key, _ => cacheHolder, (_, _) => cacheHolder);
        return value;
    }

    public TValue Set<TValue>(string key, TValue value)
    {
        return SetAsync(key, Task.FromResult(value)).GetAwaiter().GetResult();
    }

    public bool Reset(string? value = null)
    {
        if (value != null)
            return _objectCache.Remove(value, out _);
        _objectCache.Clear();
        return true;
    }


    public Task<T> GetOrSet<T>(string key, Func<Task<T>> getValue) where T : class
    {
        return GetOrSetAsync(key, getValue);
    }

    public Task<TValue>? GetAsync<TValue>(string key)
    {
        if (_objectCache.TryGetValue(key, out var values))
            if (!values.IsExpired)
                return values.AsValue<Task<TValue>>();
            else
                StartCleanup();
        return null;
    }

    public Task<TValue?> GetStaleAsync<TValue>(string key)
    {
        if (_objectCache.TryGetValue(key, out var values))
        {
            // Return even if expired; we intentionally do NOT trigger cleanup here.
            var taskValue = values.AsValue<Task<TValue?>>();
            if (taskValue != null)
                return taskValue;
        }

        return Task.FromResult<TValue?>(default);
    }

    public Task<TValue> GetOrRefreshAsync<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var addOrUpdate = _objectCache.AddOrUpdate(key,
            _ => new CacheHolder(getValue(), DateTime.Now.Add(_defaultCacheTime)),
            (_, existing) => existing
        );
        if (addOrUpdate.IsExpired)
        {
            _objectCache.AddOrUpdate(key,
                _ => new CacheHolder(getValue(), DateTime.Now.Add(_defaultCacheTime)),
                (_, existing) => existing.IsExpired
                    ? new CacheHolder(getValue(), DateTime.Now.Add(_defaultCacheTime))
                    : existing
            );
        }

        return addOrUpdate.AsValue<Task<TValue>>()!;
    }

    public TValue? Get<TValue>(string key) where TValue : class
    {
        return GetAsync<TValue>(key)?.GetAwaiter().GetResult();
    }

    private void StartCleanup()
    {
        if (DateTime.Now <= _nextExpiry) return;
        lock (_objectCache)
        {
            if (DateTime.Now <= _nextExpiry) return;
            _nextExpiry = DateTime.Now.Add(_defaultCacheTime);
            Task.Run(() =>
            {
                foreach (var cacheHolder in _objectCache.ToArray())
                    if (cacheHolder.Value.IsExpired)
                        _objectCache.TryRemove(cacheHolder.Key, out _);
            });
        }
    }

    private class CacheHolder(object value, DateTime expire)
    {
        public bool IsExpired => DateTime.Now > expire;

        internal TValue? AsValue<TValue>() where TValue : class
        {
            return value as TValue;
        }
    }
}