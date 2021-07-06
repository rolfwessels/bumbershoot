using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bumbershoot.Utilities.Helpers
{
    public static class StringHelper
    {
        public static string UriCombine(this string baseUri, params string[] addition)
        {
            var uri = baseUri;
            foreach (var moreUri in addition) uri = EnsureEndsWith(uri, "/") + EnsureDoesNotStartWith(moreUri, "/");
            return uri;
        }


        public static string ToInitialCase(this string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
        }

        public static string UnderScoreAndCamelCaseToHumanReadable(this string text)
        {
            var replace = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            return replace.Replace("_", " ");
        }

        public static string[] GetEmailAddresses(this string to)
        {
            if (string.IsNullOrEmpty(to)) return new string[0];
            return to.Split(';', ',', ' ').Where(x => x.Contains("@")).ToArray();
        }

        public static string StripHtml(this string inputHtml)
        {
            var stripHtml = Regex.Replace(inputHtml, @"<[^>]+>|&nbsp;", "");
            return Regex.Replace(stripHtml, @"\s{2,}", " ").Trim();
        }


        public static string EnsureDoesNotStartWith(this string value, string prefix)
        {
            if (value.StartsWith(prefix)) return value.Substring(prefix.Length);
            return value;
        }

        public static string EnsureEndsWith(this string value, string postFix)
        {
            if (value.EndsWith(postFix)) return value;
            return value + postFix;
        }

        public static string Mask(this string key, int length, string mask = "XXXX")
        {
            if (key == null) return key;
            return key.Substring(0, Math.Min(key.Length, length)) + mask;
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            if (base64EncodedData == null) return null;
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(this string plainText)
        {
            if (plainText == null) return null;
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string OrEmpty(this string value)
        {
            return value ?? string.Empty;
        }
    }
}