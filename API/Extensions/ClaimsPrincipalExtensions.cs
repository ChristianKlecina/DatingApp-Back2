using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
           // Console.WriteLine("00000000000000000000000000    "  + user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //Console.WriteLine("00000000000000000000000000    " + user.FindFirst(ClaimTypes.GivenName)?.Value);
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.GivenName)?.Value);
        }


    }
}
