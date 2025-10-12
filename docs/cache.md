# Caching System

The Bumbershoot.Utilities library provides a simple and extensible caching system with both in-memory and file-based implementations. The caching system is designed to be thread-safe, easy to use, and supports both synchronous and asynchronous operations.

## TL;DR

Pick one:
- Use `InMemoryCache(TimeSpan)` for fastest, process-local, non-persistent caching.
- Use `FileCache(TimeSpan, prefix)` when you want values to survive restarts.

Core calls (sync & async):
```csharp
cache.GetOrSet(key, () => value);              // sync populate
await cache.GetOrSetAsync(key, asyncFactory);  // async populate
cache.Get<T>(key);                             // null if missing / expired
await cache.GetAsync<T>(key);                  // Task<T>? (null if expired/missing)
await cache.GetStaleAsync<T>(key);             // returns expired value if still around
cache.Reset(key); / cache.Reset();             // remove one / all
```

Stale pattern (FileCache ordering matters): call `GetStaleAsync` BEFORE `GetAsync` if you might want the expired value, because `GetAsync` deletes expired files.

Minimal resilient fetch:
```csharp
var value = await cache.GetAsync<MyDto>(k)
         ?? await cache.GetStaleAsync<MyDto>(k)
         ?? await cache.GetOrSetAsync(k, LoadFresh);
```

That's it—go deeper below if you need details.

## Quick Start

For a quick overview and basic examples, see the [caching section in the README](../README.md#simple-in-memory-cache).

## Overview

The caching system consists of:

- **Two interfaces**: `ISimpleObjectCache` (sync) and `ISimpleObjectCacheASync` (async)
- **Two implementations**: `InMemoryCache` (memory-based) and `FileCache` (disk-based)
- **Thread-safe operations** with automatic cleanup
- **Configurable expiration** times
- **Generic type support** for cached values

## Implementations

### InMemoryCache

A thread-safe in-memory cache with automatic cleanup of expired entries.

**Features:**
- Thread-safe using `ConcurrentDictionary`
- Automatic cleanup of expired entries
- O(1) lookup performance
- Configurable expiration time

**Constructor:** `new InMemoryCache(TimeSpan defaultCacheTime)`

### FileCache

A file-based cache that persists values to disk using JSON serialization.

**Features:**
- Persistent across application restarts
- Thread-safe with semaphore-based concurrency control
- JSON serialization using System.Text.Json
- Stores files in temp directory with custom prefix
- Automatic cleanup of expired files

**Constructor:** `new FileCache(TimeSpan? fromMinutes = null, string prefix = "file_cache")`

**File Storage:**
- Location: `%TEMP%/{prefix}/` directory
- Format: JSON serialized objects
- Expiration: Based on file's last write time

## Basic Usage

### Core Operations

```csharp
// Create caches
var memoryCache = new InMemoryCache(TimeSpan.FromMinutes(5));
var fileCache = new FileCache(TimeSpan.FromMinutes(30), "myapp");

// Set and get values
cache.Set("user:123", user);
var cachedUser = cache.Get<User>("user:123");

// Get or set pattern (most common usage)
var user = cache.GetOrSet("user:123", () => userService.GetUser(123));

// Async operations
var user = await cache.GetOrSetAsync("user:123", async () => 
    await userService.GetUserAsync(123));

// Reset cache entries
cache.Reset("user:123"); // Remove specific entry
cache.Reset(); // Clear entire cache
```

## Performance Considerations

### InMemoryCache

- **Best for**: Frequently accessed, small to medium datasets
- **Memory usage**: Keeps all cached items in memory
- **Performance**: Fastest access, O(1) lookup
- **Scalability**: Limited by available memory

### FileCache

- **Best for**: Large datasets, data that should survive restarts
- **Disk usage**: Creates files in temp directory
- **Performance**: Slower than memory cache due to I/O operations
- **Scalability**: Limited by disk space and I/O performance

## Stale Retrieval (`GetStaleAsync`)

Sometimes you want to read a value even after its normal expiration window in order to:
 - Serve slightly stale data while a refresh happens in the background.
 - Preserve last-known-good information for diagnostics.
 - Avoid a thundering-herd on an expensive upstream dependency.

For that, the async interface exposes:

```csharp
Task<T>? GetAsync<T>(string key);        // Returns null once expired
Task<T>? GetStaleAsync<T>(string key);   // Returns value even if expired (if it still exists)
```

### Ordering Note (FileCache)
`FileCache` deletes an expired file when you call `GetAsync` after the timeout. Because `GetStaleAsync` intentionally returns the value even if expired (and does **not** delete it), call `GetStaleAsync` first if you need the stale content:

```csharp
// Correct when you want stale fallback
var stale = await cache.GetStaleAsync<MyDto>(key); // may return expired value
var current = await cache.GetAsync<MyDto>(key);    // will be null (and may delete file) if expired

// Wrong order (GetAsync first) for FileCache: the file may be deleted before stale read
```

`InMemoryCache` keeps expired entries until a periodic/threshold cleanup runs, so ordering is usually less critical there, but using the same order (stale first, then normal) keeps behavior consistent across implementations.
