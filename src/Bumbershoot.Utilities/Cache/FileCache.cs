using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Cache;

public class FileCache : ISimpleObjectCache
{
    private readonly string _path;
    private readonly TimeSpan _timeOut;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _conCurrencyCheck = new();

    public FileCache(TimeSpan? fromMinutes = null, string prefix = "file_cache")
    {
        _path = Path.Combine(Path.GetTempPath(), prefix);
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        
        _timeOut = fromMinutes ?? TimeSpan.FromMinutes(5);
    }

    public TValue GetAndReset<TValue>(string key, Func<TValue> getValue) where TValue : class
    {
        var fileName = GetFileName(key);
        if (File.Exists(fileName))
        {
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);
        }
        var found = Get<TValue>(key);
        return found ?? Set(key, getValue());
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


    public TValue Set<TValue>(string key, TValue value)
    {
        var fileName = GetFileName(key);
        File.WriteAllText(fileName, JsonSerializer.Serialize(value));
        return value;
    }

    private string GetFileName(string key)
    {
        return Path.Combine(_path, key.Replace(".","_").Replace(":","_"));
    }


    public TValue? Get<TValue>(string key) where TValue : class
    {
        var fileName = GetFileName(key);
        if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(Task<>))
        {
            throw new InvalidOperationException("Use GetOrSetAsync for Task<T> types");
        }

        if (File.Exists(fileName))
        {
            var lastWriteTimeUtc = DateTime.UtcNow - File.GetLastWriteTimeUtc(fileName);
            if (lastWriteTimeUtc < _timeOut)
            {
                return JsonSerializer.Deserialize<TValue>(File.ReadAllText(fileName));
            }
            File.Delete(fileName);
        }

        return null;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getValue)
    {
        var found = GetAsync<T>(key);
        if (found != null)
        {
            return await found;
        }
        var fileName = GetFileName(key);
        var semaphore = _conCurrencyCheck.GetOrAdd(fileName, new SemaphoreSlim(1,1));
        try
        {
            await semaphore.WaitAsync();
            found = GetAsync<T>(key);
            if (found != null)
            {
                return await found;
            }
            var value = await getValue();
            Set(key, value);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public Task<TValue>? GetAsync<TValue>(string key)
    {
        var fileName = GetFileName(key);
        if (File.Exists(fileName))
        {
            var lastWriteTimeUtc = DateTime.UtcNow - File.GetLastWriteTimeUtc(fileName);
            if (lastWriteTimeUtc < _timeOut)
            {
                return Task.Run(() =>
                {
                    var readAllText = File.ReadAllText(fileName);
                    var deserialize = JsonSerializer.Deserialize<TValue>(readAllText);
                    if (deserialize == null) throw new InvalidOperationException("Deserialized value is null");
                    return deserialize;
                });
            }
            File.Delete(fileName);
        }

        return null;
    }

    public Task<T> GetOrSet<T>(string value, Func<Task<T>> getValue) where T : class
    {
        return GetOrSetAsync(value, getValue);
    }

    public T GetOrSet<T>(string value, Func<T> getValue) where T : class
    {
        var found = Get<T>(value);
        return found ?? Set(value, getValue());
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