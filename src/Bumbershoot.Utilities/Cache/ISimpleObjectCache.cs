using System;

namespace Bumbershoot.Utilities.Cache
{
    public interface ISimpleObjectCache
    {
        TValue Get<TValue>(string key, Func<TValue> getValue) where TValue : class;
        TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class;
        TValue Set<TValue>(string key, TValue value);
        TValue Get<TValue>(string key) where TValue : class;
    }
}