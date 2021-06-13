using System.Security.Claims;

namespace Matchmaker.Helpers
{
    public static class ClaimPrincipalExtension
    {
        public static string GetName(this ClaimsPrincipal principal)
            => ((ClaimsIdentity)principal.Identity).FindFirst(ClaimTypes.Name)?.Value;

        public static int GetId(this ClaimsPrincipal principal) 
            => int.Parse(principal.GetName());
    }
}
