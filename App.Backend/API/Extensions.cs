using System.Security.Claims;
using App.Backend.Core;

namespace App.Backend.API;

public static class Extensions
{
    extension(ClaimsPrincipal principal)
    {
        public Guid GetSID()
        {
            string? claim = principal.FindFirstValue("admin_user_id")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(claim, out var guid) ?
                guid :
                throw new ServiceException(403, "Unable to verify user");
        }

        public Guid GetLocalRealmSID()
        {
            string? claim = principal.FindFirstValue(ClaimTypes.NameIdentifier); // sub
            return Guid.TryParse(claim, out var guid) ?
                guid :
                throw new ServiceException(403, "Unable to verify user");
        }
    }
}
