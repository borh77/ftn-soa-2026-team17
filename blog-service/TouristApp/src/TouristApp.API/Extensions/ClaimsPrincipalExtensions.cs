using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static long PersonId(this ClaimsPrincipal user)
    {
        return long.Parse(
            user.Claims.First(
                c => c.Type == "personId").Value);
    }
}