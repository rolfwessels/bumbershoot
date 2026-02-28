# Bumbershoot

General C# utils

## Some examples

### Strongly typed configuration

I like strongly typed app settings and configurations. The following class allows me to easily setup a strongly typed class that has defaults and can read from all appsettings.

```csharp
public class TestSettings : BaseSettings
{
    private static Lazy<TestSettings> _instance = new(() => new TestSettings(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true).Build()));

    public TestSettings(IConfiguration configuration) : base(configuration, null)
    {
    }

    public static TestSettings Instance => _instance.Value;
    public string SampleSettings => ReadConfigValue("SampleSettings", "Bumbershoot-Sample");
    public bool BoolValue => ReadConfigValue("BoolValue", false);
    public int IntValue => ReadConfigValue("IntValue", 1);


}
// Usages

Console.Out(TestSettings.Instance.BoolValue); // Writes "False"
Console.Out(TestSettings.Instance.SampleSettings); // Writes "Bumbershoot-Sample" (unless the json has other value)
```

### Encrypted settings for sensitive data

For sensitive configuration values (API keys, passwords, connection strings), I use `BaseSettingsWithEncryption`. This allows storing **encrypted values in source code** while keeping the decryption key external (environment variables, secret managers, etc.).

```csharp
public class AppSettings : BaseSettingsWithEncryption
{
    public AppSettings(IConfiguration configuration)
        : base(configuration, "AppSettings", "EncryptionKey")
    {
    }

    // Unencrypted configuration
    public string Environment => ReadConfigValue("Environment", "Development");

    // Encrypted configuration (automatically decrypted on read)
    public string DatabasePassword => ReadConfigValue("DatabasePassword", "");
    public string ApiKey => ReadConfigValue("ApiKey", "");
}
```

Store encrypted values in `appsettings.json`:

```json
{
  "AppSettings": {
    "Environment": "Production",
    "DatabasePassword": "EN|AQIDANz5OLvq1HRbtall...",
    "ApiKey": "EN|AQIDANz5OLvq1HRbtall..."
  },
  "EncryptionKey": "your-secret-key-here"
}
```

📖 **[Complete Encrypted Settings Documentation](docs/encryption-settings.md)** - Comprehensive guide covering key management, encryption/decryption, security best practices, examples, and troubleshooting.

### Simple in memory cache

I wanted a very simple extensible in memory cache.

```csharp
var inMemoryCache = new InMemoryCache(TimeSpan.FromSeconds(1));
var result1 = inMemoryCache.GetOrSet("value", () => "one");
var result2 = inMemoryCache.GetOrSet("value", () => "two");
// assert
result1.Should().Be("one");
result2.Should().Be("one"); // because value is already in the cache
```

📖 **[Complete Caching Documentation](docs/cache.md)** - Comprehensive guide covering both InMemoryCache and FileCache implementations, API reference, best practices, and usage patterns.

### Aes encryption

I wanted a Aes encryption where the key can be any length and it works with string input and output

```csharp
var simpleAes = new SimpleAes();
var encrypt = simpleAes.Encrypt("key","hello");
var decrypt = simpleAes.Decrypt("key", encrypt);
// assert
decrypt.Should().Be("hello");
```

### Retry

I often want to retry an action when needed and maybe back off on the retry delay.

```csharp
var retries = 4;
var retryDelay = 10;
// action
async Task CallToRetry()
{
    await Task.Delay(1);
    throw new ArgumentException();
}
void CallBack(ArgumentException _, int i) => Console.WriteLine($"WARN: Failed with '{_.Message}', will retry in {i}ms.");
try
{
    await TaskHelper.RetryAsync<ArgumentException>(CallToRetry, retries, retryDelay, CallBack);
}
catch (Exception e)
{
    Console.WriteLine($"Error: {e.Message}");
}

// output
//WARN: Failed with 'Value does not fall within the expected range.', will retry in 10ms.
//WARN: Failed with 'Value does not fall within the expected range.', will retry in 20ms.
//WARN: Failed with 'Value does not fall within the expected range.', will retry in 40ms.
//Error: Value does not fall within the expected range.

```

## Getting started with development

Open the docker environment to do all development and deployment

```bash
# bring up dev environment
make build up
# to test the app
make test
# to run the app
make start
```

## Available make commands

### Commands outside the container

- `make up` : brings up the container & attach to the default container
- `make down` : stops the container
- `make build` : builds the container

### Commands to run inside the container

- `make start` : Run the app
- `make test` : To test the app
