using System;

namespace Bumbershoot.Utilities.Cache
{
    public interface ISimpleObjectCache
    {
        TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class;
        TValue Set<TValue>(string key, TValue value);
        TValue? Get<TValue>(string key) where TValue : class;
        T GetOrSet<T>(string value, Func<T> getValue) where T : class;
        void Reset(string? value = null);
    }
}