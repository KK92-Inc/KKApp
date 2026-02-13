using System.Text.RegularExpressions;

namespace App.Backend.Core;

public static partial class Extensions
{
    extension(string value)
    {
        /// <summary>
        /// Generate a slug out of a string
        /// </summary>
        /// <returns>The string but slugified</returns>
        public string ToSlug()
        {
            var v = Initial().Replace(value, "-").ToLower();
            return UrlPattern().Replace(v, string.Empty);
        }
    }

    [GeneratedRegex(@"\s")]
    private static partial Regex Initial();

    [GeneratedRegex(@"[^a-zA-Z0-9-.]")]
    private static partial Regex UrlPattern();
}
