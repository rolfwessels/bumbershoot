using System;
using System.Linq;
using System.Reflection;

namespace Bumbershoot.Utilities.Helpers
{
    public static class EmbededResourceHelper
    {
        public static string ReadResource(string resourceName, Assembly getExecutingAssembly)
        {
            var assembly = getExecutingAssembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException(
                        $"{resourceName} resource does not exist in {getExecutingAssembly.FullName.Split(',').First()} assembly.");
                return stream.ReadToString();
            }
        }

        public static string ReadResource(string resourceName, Type type)
        {
            return ReadResource(resourceName, type.GetTypeInfo().Assembly);
        }
    }
}