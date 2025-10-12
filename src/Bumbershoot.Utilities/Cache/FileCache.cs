using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Cache;

public class FileCache : ISimpleObjectCacheASync, ISimpleObjectCache
{
    private readonly string _path;
    private readonly TimeSpan _timeOut;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _conCurrencyCheck = new();

    public FileCache(TimeSpan? fromMinutes = null, string prefix = "file_cache")
    {
        _path = Path.Combine(Path.GetTempPath(), prefix);
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        _timeOut = fromMinutes ?? TimeSpan.FromMinutes(5);
    }


    public Task<TValue> GetAndResetAsync<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var fileName = GetFileName(key);
        if (File.Exists(fileName))
        {
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);
        }

        return GetOrSetAsync(key, getValue);
    }

    public Task<TValue> GetOrSet<TValue>(string key, Func<Task<TValue>> getValue) where TValue : class
    {
        return GetOrSetAsync(key, getValue);
    }


    private string GetFileName(string key)
    {
        var replace = Regex.Replace(key, "[^a-zA-Z0-9_.-]", "_") + ".json";
        var i = 100;
        if (replace.Length > i)
        {
            var hash = key.GetHashCode().ToString("X");
            replace = replace.Substring(0, i - hash.Length) + hash + ".json";
        }

        return Path.Combine(_path, replace);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValue)
    {
        var found = await GetAsync<T>(key);
        if (found != null)
        {
            return found;
        }

        var fileName = GetFileName(key);
        var semaphore = _conCurrencyCheck.GetOrAdd(fileName, new SemaphoreSlim(1, 1));
        try
        {
            await semaphore.WaitAsync();
            found = await GetAsync<T>(key);
            if (found != null)
            {
                return found;
            }

            var value = getValue();
            await SetAsync(key, value);
            _conCurrencyCheck.Remove(fileName, out _);
            return await value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<T> SetAsync<T>(string key, Task<T> task)
    {
        var fileName = GetFileName(key);
        var value = await task;
        var serialize = JsonSerializer.Serialize(value);
        await File.WriteAllTextAsync(fileName, serialize);
        return value;
    }

    public async Task<TValue?> GetAsync<TValue>(string key)
    {
        return await GetValue<TValue>(key, freshOnly: true);
    }

    private async Task<TValue?> GetValue<TValue>(string key, bool freshOnly)
    {
        var fileName = GetFileName(key);
        if (File.Exists(fileName))
        {
            var lastWriteTimeUtc = DateTime.UtcNow - File.GetLastWriteTimeUtc(fileName);
            if (lastWriteTimeUtc < _timeOut || !freshOnly)
            {
                return await Task.Run(() =>
                {
                    var readAllText = File.ReadAllText(fileName);
                    var deserialize = JsonSerializer.Deserialize<TValue>(readAllText);
                    if (deserialize == null) throw new InvalidOperationException("Deserialized value is null");
                    return deserialize;
                });
            }

            if (freshOnly)
            {
                File.Delete(fileName);
            }
        }

        return default;
    }

    public Task<TValue?> GetStaleAsync<TValue>(string key)
    {
        return GetValue<TValue>(key, freshOnly: false);
    }

    public async Task<TValue> GetOrRefreshAsync<TValue>(string key, Func<Task<TValue>> getValue)
    {
        var readStale = await GetValue<TValue>(key, freshOnly: false);
        if (readStale == null)
        {
            return await GetOrSetAsync(key, getValue);
        }

        var lastWriteTimeUtc = DateTime.UtcNow - File.GetLastWriteTimeUtc(GetFileName(key));
        if (lastWriteTimeUtc > _timeOut)
        {
            await GetOrSetAsync(key, getValue);
        }


        return readStale;
    }

    public TValue Set<TValue>(string key, TValue value)
    {
        return SetAsync(key, Task.FromResult(value)).GetAwaiter().GetResult();
    }

    public TValue? Get<TValue>(string key) where TValue : class
    {
        return GetAsync<TValue>(key)?.GetAwaiter().GetResult();
    }

    public TValue GetOrSet<TValue>(string key, Func<TValue> getValue) where TValue : class
    {
        return GetOrSetAsync(key, () => Task.FromResult(getValue())).GetAwaiter().GetResult();
    }

    public TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class
    {
        return GetAndResetAsync(key, () => Task.FromResult(getValue())).GetAwaiter().GetResult();
    }

    public bool Reset(string? value = null)
    {
        if (value == null)
        {
            Directory.Delete(_path, true);
            Directory.CreateDirectory(_path);
            return true;
        }

        var fileName = GetFileName(value);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
            return true;
        }

        return false;
    }
}