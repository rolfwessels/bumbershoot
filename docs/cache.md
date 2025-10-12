# Caching System

The Bumbershoot.Utilities library provides a simple and extensible caching system with both in-memory and file-based implementations. The caching system is designed to be thread-safe, easy to use, and supports both synchronous and asynchronous operations.

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
