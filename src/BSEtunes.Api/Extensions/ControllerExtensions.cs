using System.Security.Claims;

namespace BSEtunes.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                ?.Value;
        }
    }
}