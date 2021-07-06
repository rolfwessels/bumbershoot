using System;
using System.Collections.Generic;

namespace Bumbershoot.Utilities.Helpers
{
    public static class DictionaryHelper
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> contextContextData, TKey key,
            Func<TValue> value)
        {
            if (contextContextData == null) throw new ArgumentNullException(nameof(contextContextData));
            if (contextContextData.TryGetValue(key, out var result))
                return result;
            else
                lock (contextContextData)
                {
                    if (contextContextData.TryGetValue(key, out var result1)) return result1;
                    var value1 = value();
                    contextContextData.Add(key, value1);
                    return value1;
                }
        }
    }
}