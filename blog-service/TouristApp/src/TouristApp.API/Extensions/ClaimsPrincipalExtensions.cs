using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static long PersonId(this ClaimsPrincipal user)
    {
        var personIdClaim = user.Claims.FirstOrDefault(c => c.Type == "personId")?.Value;
        if (personIdClaim != null && long.TryParse(personIdClaim, out var personId))
            return personId;

        var sub = user.Claims.FirstOrDefault(c => c.Type == "sub"
                   || c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (sub != null && long.TryParse(sub, out var subId))
            return subId;

        return 1; // fallback
    }
}