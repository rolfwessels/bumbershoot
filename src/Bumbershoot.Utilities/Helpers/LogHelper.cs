using System;
using System.Text.Json;

namespace Bumbershoot.Utilities.Helpers
{
    public static class LogHelper
    {
        public static string Dump(this object val)
        {
            return Dump(val, true);
        }

        public static string Dump(this object val, bool indented)
        {

            return JsonSerializer.Serialize(val, new JsonSerializerOptions { WriteIndented = indented });
        }

        public static T Dump<T>(this T val, string description)
        {
            Console.Out.WriteLine(description + ":" + val.Dump());
            return val;
        }
    }
}