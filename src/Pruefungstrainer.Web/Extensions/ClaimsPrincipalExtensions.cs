using System.Security.Claims;

namespace Pruefungstrainer.Web.Extensions;

/// <summary>
/// Hilfsmethoden, um aus dem angemeldeten Benutzer (User) die eigene Benutzer-Id zu lesen.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Die Id aus dem Claim "NameIdentifier", den wir beim Login setzen (siehe AccountController).
    /// </summary>
    public static int GetBenutzerId(this ClaimsPrincipal user)
    {
        var wert = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(wert, out var id)
            ? id
            : throw new InvalidOperationException("Kein angemeldeter Benutzer.");
    }
}
