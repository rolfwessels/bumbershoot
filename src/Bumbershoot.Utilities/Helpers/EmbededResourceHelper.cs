using System;
using System.Linq;
using System.Reflection;

namespace Bumbershoot.Utilities.Helpers;

public static class EmbeddedResourceHelper
{
    public static string ReadResource(string resourceName, Assembly getExecutingAssembly)
    {
        var assembly = getExecutingAssembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            var manifestResourceNames = assembly.GetManifestResourceNames().Take(4);
            throw new ArgumentException(
                $"{resourceName} resource does not exist in {getExecutingAssembly.FullName.OrEmpty().Split(',').First()} assembly. Did you mean [{manifestResourceNames.StringJoin()}]?");
        }

        return stream.ReadToString();
    }

    public static string ReadResource(string resourceName, Type type)
    {
        return ReadResource(resourceName, type.GetTypeInfo().Assembly);
    }
}