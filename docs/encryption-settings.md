# Encrypted Settings Configuration

## Overview

`BaseSettingsWithEncryption` extends the standard configuration pattern to automatically encrypt and decrypt sensitive values stored in your source code. This approach allows you to:

- **Store encrypted secrets** in source code (safe for private repos)
- **Keep the decryption key external** (environment variable, local file, or secret manager)
- **Reduce key management complexity** (one key per project/environment instead of many)
- **Maintain type safety** with strongly-typed configuration classes

## The Problem It Solves

When building applications, you often need to store configuration values that contain sensitive data (API keys, connection strings, passwords, etc.). Traditional approaches require:

- Multiple secrets scattered across different locations
- Complex key management (which key is used where?)
- Risk of accidentally committing unencrypted secrets to version control

`BaseSettingsWithEncryption` simplifies this by:

1. Storing **encrypted values directly in source code** with a prefix (e.g., `EN|...`)
2. Keeping **one encryption key** outside the repository (per project/environment)
3. Automatically **decrypting on read** when you access the configuration

## Basic Usage

### Step 1: Create Your Settings Class

```csharp
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;

public class AppSettings : BaseSettingsWithEncryption
{
    public AppSettings(IConfiguration configuration)
        : base(
            configuration,
            configGroup: "AppSettings",      // Section in appsettings.json
            keyGroup: "EncryptionKey"         // Where the decryption key is stored
        )
    {
    }

    // Regular unencrypted settings
    public string Environment => ReadConfigValue("Environment", "Development");

    // Encrypted settings (automatically decrypted on read)
    public string DatabasePassword => ReadConfigValue("DatabasePassword", "");
    public string ApiKey => ReadConfigValue("ApiKey", "");
}
```

### Step 2: Add to Dependency Injection

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register settings
builder.Services.AddSingleton<AppSettings>(sp =>
    new AppSettings(builder.Configuration)
);

var app = builder.Build();
```

### Step 3: Encrypt Your Values

First, generate encrypted values using `GetEncryptedValue`:

```csharp
var encryption = new SimpleAes();
var key = "your-secret-key-here";
var plaintext = "my-database-password";
var encrypted = "EN|" + encryption.Encrypt(key, plaintext);

Console.WriteLine(encrypted);
// Output: EN|...encrypted-gibberish...
```

Or use this helper in a utility class:

```csharp
public static class EncryptionHelper
{
    public static string GenerateEncryptedValue(string key, string plaintext)
    {
        var encryption = new SimpleAes();
        return "EN|" + encryption.Encrypt(key, plaintext);
    }
}

// Usage:
var encrypted = EncryptionHelper.GenerateEncryptedValue("my-key", "secret-value");
```

### Step 4: Store Encrypted Values in appsettings.json

```json
{
  "AppSettings": {
    "Environment": "Production",
    "DatabasePassword": "EN|AQIDANz5OLvq1HRbtallQVZ...",
    "ApiKey": "EN|AQIDANz5OLvq1HRbtallQVZ..."
  },
  "EncryptionKey": "your-secret-key-here"
}
```

### Step 5: Use It in Your Code

```csharp
public class MyService
{
    private readonly AppSettings _settings;

    public MyService(AppSettings settings)
    {
        _settings = settings;
    }

    public void Connect()
    {
        // This is automatically decrypted
        var password = _settings.DatabasePassword;
        var apiKey = _settings.ApiKey;

        // Use them...
    }
}
```

## Advanced Configuration

### Custom Prefix

By default, encrypted values use the prefix `EN|`. You can change this:

```csharp
public class AppSettings : BaseSettingsWithEncryption
{
    public AppSettings(IConfiguration configuration)
        : base(
            configuration,
            configGroup: "AppSettings",
            keyGroup: "EncryptionKey",
            prefix: "ENCRYPTED|"  // Custom prefix
        )
    {
    }
}
```

### Custom Encryption

By default, `SimpleAes` is used. You can provide your own implementation of `ISimpleEncryption`:

```csharp
public class AppSettings : BaseSettingsWithEncryption
{
    public AppSettings(IConfiguration configuration, ISimpleEncryption customEncryption)
        : base(
            configuration,
            configGroup: "AppSettings",
            keyGroup: "EncryptionKey",
            encryption: customEncryption
        )
    {
    }
}
```

### Environment-Specific Keys

Store different encryption keys for different environments:

**appsettings.Development.json:**
```json
{
  "EncryptionKey": "dev-key-12345"
}
```

**appsettings.Production.json:**
```json
{
  "EncryptionKey": "prod-key-67890"
}
```

The framework automatically selects the right key based on the active environment.

### Key Rotation

To rotate an encryption key:

1. Add encrypted values using the new key with a different key group
2. Create a migration service to re-encrypt values
3. Update configuration to use the new key group

Example:

```csharp
public class AppSettings : BaseSettingsWithEncryption
{
    private readonly BaseSettingsWithEncryption _legacySettings;

    public AppSettings(IConfiguration configuration)
        : base(configuration, "AppSettings", "EncryptionKeyNew")
    {
        // Keep old settings for migration
        _legacySettings = new BaseSettingsWithEncryption(
            configuration, "AppSettings", "EncryptionKeyLegacy"
        );
    }
}
```

## How It Works

### Reading Encrypted Values

When you call `ReadConfigValue("DatabasePassword", defaultValue)`:

1. `BaseSettingsWithEncryption` overrides `ReadConfigValue`
2. It reads the raw value from configuration
3. If the value starts with the prefix (e.g., `EN|`), it's decrypted
4. If not, it's returned as-is (unencrypted values are allowed)
5. If the key is missing, the default value is returned

### Encryption/Decryption

The default `SimpleAes` class:

- Uses **AES encryption** with automatic padding
- Accepts keys of **any length** (internally hashed to match AES requirements)
- Works with **string input and output** (Base64 encoded for storage)
- Is **deterministic** (same plaintext + key = same ciphertext)

## Security Considerations

### ✅ Best Practices

- **Keep the encryption key in environment variables** or secret managers, not in source control
- **Use strong, random keys** (at least 32 characters for AES-256 equivalent)
- **Rotate keys periodically** following your organization's security policies
- **Audit access** to encryption keys via monitoring/logging
- **Use HTTPS** when transmitting encrypted values over the network
- **Encrypt the entire configuration file** if your infrastructure allows it

### ⚠️ Limitations

- **Not suitable for client-side applications** (the key would be exposed in binaries)
- **Encryption provides confidentiality, not integrity** (can't detect tampering)
- **Performance overhead** is minimal but non-zero (encryption happens on config read)
- **The key must be available at runtime** (you can't run without access to it)

## Examples

### Database Connection String

```csharp
public class DatabaseSettings : BaseSettingsWithEncryption
{
    public DatabaseSettings(IConfiguration configuration)
        : base(configuration, "Database", "DbEncryptionKey")
    {
    }

    public string ConnectionString => ReadConfigValue("ConnectionString", "");
    public int CommandTimeout => ReadConfigValue("CommandTimeout", 30);
}
```

**appsettings.json:**
```json
{
  "Database": {
    "ConnectionString": "EN|AQIDANz5OLvq1HRbtall...",
    "CommandTimeout": "60"
  },
  "DbEncryptionKey": "secure-key-here"
}
```

### Third-Party API Keys

```csharp
public class ApiSettings : BaseSettingsWithEncryption
{
    public ApiSettings(IConfiguration configuration)
        : base(configuration, "APIs", "ApiEncryptionKey")
    {
    }

    public string SendGridApiKey => ReadConfigValue("SendGridApiKey", "");
    public string TwilioAuthToken => ReadConfigValue("TwilioAuthToken", "");
    public string StripeSecretKey => ReadConfigValue("StripeSecretKey", "");
}
```

### Mixed Encrypted and Unencrypted

```csharp
public class MixedSettings : BaseSettingsWithEncryption
{
    public MixedSettings(IConfiguration configuration)
        : base(configuration, "App", "MasterKey")
    {
    }

    // Public configuration (unencrypted)
    public string AppName => ReadConfigValue("AppName", "MyApp");
    public string LogLevel => ReadConfigValue("LogLevel", "Information");

    // Sensitive data (encrypted)
    public string AdminPassword => ReadConfigValue("AdminPassword", "");
    public string JwtSecret => ReadConfigValue("JwtSecret", "");
}
```

## Testing

### Unit Tests

```csharp
[TestFixture]
public class AppSettingsTests
{
    [Test]
    public void Should_Decrypt_Encrypted_Values()
    {
        // Arrange
        var encryption = new SimpleAes();
        var key = "test-key-12345";
        var encryptedValue = "EN|" + encryption.Encrypt(key, "secret");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "AppSettings:DatabasePassword", encryptedValue },
                { "EncryptionKey", key }
            })
            .Build();

        // Act
        var settings = new AppSettings(config);

        // Assert
        settings.DatabasePassword.Should().Be("secret");
    }

    [Test]
    public void Should_Return_Default_For_Missing_Key()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "EncryptionKey", "test-key" }
            })
            .Build();

        // Act
        var settings = new AppSettings(config);

        // Assert
        settings.DatabasePassword.Should().Be(""); // default value
    }
}
```

## Troubleshooting

### "Config required the {keyGroup} to be set"

**Problem:** The encryption key is missing from configuration.

**Solution:** Ensure your appsettings file includes the encryption key:
```json
{
  "EncryptionKey": "your-key-here"
}
```

Or set it as an environment variable (if using environment variables in config).

### "Cryptographic key size is not valid"

**Problem:** Your decryption key doesn't match the one used to encrypt.

**Solution:** Verify you're using the correct key and that the encrypted value was created with that key.

### Decrypted Value is Corrupted

**Problem:** The encrypted value is partially or incorrectly decrypted.

**Solution:**
- Check for typos in the encrypted value
- Ensure the prefix is correct (default: `EN|`)
- Verify the encryption key hasn't changed
- Re-encrypt the value using the current key

## See Also

- [SimpleAes Encryption Documentation](./encryption.md)
- [BaseSettings (Parent Class) Documentation](./settings.md)
- [Complete Caching Documentation](./cache.md)

---

**Last Updated:** 2026-02-28  
**Maintainer:** Rolf Wessels
