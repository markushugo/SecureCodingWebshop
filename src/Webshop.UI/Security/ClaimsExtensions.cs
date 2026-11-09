using System.Security.Claims;

namespace Webshop.UI.Security;

/// <summary>Gør det nemt at hente oplysninger fra den indloggede bruger.</summary>
public static class ClaimsExtensions
{
    /// <summary>Henter bruger-id'et fra loginoplysningerne.</summary>
    public static int UserId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : throw new InvalidOperationException("Brugeridentiteten mangler.");
}
