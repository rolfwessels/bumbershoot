using System;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Cache;

public interface ISimpleObjectCacheASync
{
    Task<TValue>? GetAsync<TValue>(string key);
    Task<TValue> GetOrSetAsync<TValue>(string key, Func<Task<TValue>> getValue);
    Task<TValue> GetAndResetAsync<TValue>(string key, Func<Task<TValue>> getValue);
    Task<TValue> GetOrSet<TValue>(string key, Func<Task<TValue>> getValue) where TValue : class;
    bool Reset(string? value = null);
}