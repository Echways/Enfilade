using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Enfilade;

internal static class SectionAccess
{
    public static async Task<bool> AllowsAsync(
        Section section, ClaimsPrincipal user, EnfiladeOptions options, IAuthorizationPolicyProvider policies, IAuthorizationService authorization)
    {
        if (section.Anonymous)
            return true;
        if (options.Guards(section.Page) && user.Identity?.IsAuthenticated != true)
            return false;
        if (section.Requirements.Count == 0)
            return true;

        var policy = await AuthorizationPolicy.CombineAsync(policies, section.Requirements);
        return policy is null || (await authorization.AuthorizeAsync(user, policy)).Succeeded;
    }
}
