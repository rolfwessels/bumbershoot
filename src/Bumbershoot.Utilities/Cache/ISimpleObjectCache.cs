using System;

namespace Bumbershoot.Utilities.Cache;

public interface ISimpleObjectCache
{
    TValue Set<TValue>(string key, TValue value);
    TValue? Get<TValue>(string key) where TValue : class;
    TValue GetOrSet<TValue>(string key, Func<TValue> getValue) where TValue : class;
    TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class;
    bool Reset(string? value = null);
}