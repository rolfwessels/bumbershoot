using System;
using System.Collections.Generic;

namespace Bumbershoot.Utilities.Helpers
{
    public class EnumHelper
    {
        public static IEnumerable<T> ToArray<T>()
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values) yield return (T)value;
        }

        public static IEnumerable<string> Values<T>()
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values) yield return value.ToString()??"";
        }
    }
}