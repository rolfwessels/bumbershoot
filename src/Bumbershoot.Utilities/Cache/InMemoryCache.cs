using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Cache
{
    public class InMemoryCache : ISimpleObjectCache
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

        #region ISimpleObjectCache Members

        public TValue Get<TValue>(string key, Func<TValue> getValue) where TValue : class
        {
            var retrievedValue = Get<TValue>(key);
            if (retrievedValue == null)
            {
                var result = getValue();
                if (result != null) Set(key, result);
                return result;
            }

            return retrievedValue;
        }

        public TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class
        {
            if (_objectCache.TryGetValue(key, out var values))
                if (!values.IsExpired)
                    return values.AsValue<TValue>();
            return Set(key, getValue());
        }

        public TValue Set<TValue>(string key, TValue value)
        {
            var cacheHolder = new CacheHolder(value, DateTimeOffset.Now.Add(_defaultCacheTime));
            _objectCache.AddOrUpdate(key, s => cacheHolder, (s, holder) => cacheHolder);
            return value;
        }

        public T GetOrSet<T>(string value, Func<T> func) where T : class
        {
            var cacheHolder = _objectCache.GetOrAdd(value, s => new CacheHolder(func(), DateTimeOffset.Now.Add(_defaultCacheTime)));
            if (!cacheHolder.IsExpired)
                return cacheHolder.AsValue<T>();
            return Set(value, func());
        }

        public TValue Get<TValue>(string key) where TValue : class
        {
            if (_objectCache.TryGetValue(key, out var values))
                if (!values.IsExpired)
                    return values.AsValue<TValue>();
                else
                    StartCleanup();
            return null;
        }

        #endregion

        #region Private Methods

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

        #endregion

        #region Nested type: CacheHolder

        private class CacheHolder
        {
            private readonly DateTimeOffset _expire;
            private readonly object _value;

            public CacheHolder(object value, DateTimeOffset expire)
            {
                _value = value;
                _expire = expire;
            }

            public bool IsExpired => DateTime.Now > _expire;

            internal TValue AsValue<TValue>() where TValue : class
            {
                return _value as TValue;
            }
        }

        #endregion

       
    }
}