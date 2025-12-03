using System.Security.Claims;
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

}
