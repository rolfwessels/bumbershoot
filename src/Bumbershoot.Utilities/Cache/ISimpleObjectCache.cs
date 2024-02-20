using System;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Cache
{
    public interface ISimpleObjectCache 
    {
        TValue Set<TValue>(string key, TValue value);
        TValue? Get<TValue>(string key) where TValue : class;
        Task<TValue> GetOrSetAsync<TValue>(string key, Func<Task<TValue>> getValue);
        TValue GetOrSet<TValue>(string value, Func<TValue> getValue) where TValue : class;
        TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class;
        Task<TValue> GetAndResetAsync<TValue>(string key, Func<Task<TValue>> getValue);
        bool Reset(string? value = null);
        Task<TValue>? GetAsync<TValue>(string key);
        Task<T> GetOrSet<T>(string value, Func<Task<T>> getValue) where T : class;
    }
}