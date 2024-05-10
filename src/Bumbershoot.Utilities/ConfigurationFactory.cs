using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Bumbershoot.Utilities;

public static class ConfigurationFactory
{
    public static IConfiguration Load()
    {
        return new ConfigurationBuilder().WithJsonAndEnvVariables().Build();
    }

    public static IConfigurationBuilder WithJsonAndEnvVariables(this IConfigurationBuilder builder, string? env = null)
    {
        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env ?? GetEnvString().ToLower()}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    }

    public static string GetEnvString()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "local";
    }

    public static string InformationalVersion()
    {
        var infoVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        return $@"{infoVersion}";
    }
}