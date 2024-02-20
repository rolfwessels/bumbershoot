using System;
using System.Text.Json;

namespace Bumbershoot.Utilities.Helpers
{
    public static class LogHelper
    {
        public static T Dump<T>(this T val, string description="")
        {
            var prefix = string.IsNullOrEmpty(description)?"":(description + ":");
            Console.Out.WriteLine(prefix + JsonSerializer.Serialize<object>(val, new JsonSerializerOptions { WriteIndented = true }));
            return val;
        }
    }
}