using System.Security.Claims;
using System.Text.RegularExpressions;
using App.Backend.Core;

namespace App.Backend.API;

public static class Extensions
{
    extension(ClaimsPrincipal principal)
    {
        /// <summary>
        /// Get the current user sid.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public Guid GetSID()
        {
            string? claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var guid) ?
                guid :
                throw new ServiceException(403, "Unable to verify user");
        }
    }

    extension(string value)
    {
        /// <summary>
        /// Generate a slug out of a string
        /// </summary>
        /// <returns>The string but slugified</returns>
        public string ToSlug()
        {
            return value
                .ToLower()
                .Replace(@"[^a-z0-9\s-]", string.Empty) // Invalid chars
                .Replace(@"\s+", " ") // Convert multiple spaces into one space
                .Trim()
                .Replace(@"\s", "-"); // Hyphens
        }
    }

}
