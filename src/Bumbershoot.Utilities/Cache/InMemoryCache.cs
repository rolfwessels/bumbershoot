using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
            if (!values.IsExpired)
            {
                var asValue = values.AsValue<TValue>();
                if (asValue != null)
                    return asValue;
            }

        var andReset = Set(key, getValue());
        StartCleanup();
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
        var cacheHolder =
            _objectCache.GetOrAdd(key, _ => new CacheHolder(func(), DateTime.Now.Add(_defaultCacheTime)));
        if (cacheHolder.IsExpired)
            StartCleanup();
        else
            return cacheHolder.AsValue<T>()!;
        return Set(key, func());
    }

    public TValue Set<TValue>(string key, TValue value)
    {
        Debug.Assert(value != null, nameof(value) + " != null");
        var cacheHolder = new CacheHolder(value, DateTime.Now.Add(_defaultCacheTime));
        _objectCache.AddOrUpdate(key, _ => cacheHolder, (_, _) => cacheHolder);
        return value;
    }

    public bool Reset(string? value = null)
    {
        if (value != null)
            return _objectCache.Remove(value, out _);
        _objectCache.Clear();
        return true;
    }

    public Task<TValue>? GetAsync<TValue>(string key)
    {
        return Get<Task<TValue>>(key);
    }

    public Task<T> GetOrSet<T>(string key, Func<Task<T>> getValue) where T : class
    {
        return GetOrSetAsync(key, getValue);
    }

    public TValue? Get<TValue>(string key) where TValue : class
    {
        if (_objectCache.TryGetValue(key, out var values))
            if (!values.IsExpired)
                return values.AsValue<TValue>();
            else
                StartCleanup();
        return null;
    }

    private void StartCleanup()
    {
        if (DateTime.Now > _nextExpiry)
            lock (_objectCache)
            {
                if (DateTime.Now > _nextExpiry)
                {
                    _nextExpiry = DateTime.Now.Add(_defaultCacheTime);
                    Task.Run(() =>
                    {
                        foreach (var cacheHolder in _objectCache.ToArray())
                            if (cacheHolder.Value.IsExpired)
                                _objectCache.TryRemove(cacheHolder.Key, out _);
                    });
                }
            }
    }

    private class CacheHolder
    {
        private readonly DateTime _expire;
        private readonly object _value;

        public CacheHolder(object value, DateTime expire)
        {
            _value = value;
            _expire = expire;
        }

        public bool IsExpired => DateTime.Now > _expire;

        internal TValue? AsValue<TValue>() where TValue : class
        {
            return _value as TValue;
        }
    }
}