using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace PMBAdmin.Authorization;

/// <summary>
/// Dynamically creates authorization policies for permission claims.
/// Any policy name matching a defined permission automatically requires
/// a claim of type "Permission" with that value.
/// </summary>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private static readonly HashSet<string> _permissionValues =
        new(AppPermissions.All.Select(p => p.Value));

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
            return policy;

        if (_permissionValues.Contains(policyName))
        {
            return new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    if (context.User.HasClaim(AppPermissions.SuperUserClaimType, bool.TrueString))
                        return true;

                    if (context.User.HasClaim(AppPermissions.ClaimType, policyName))
                        return true;

                    var managePermission = AppPermissions.GetManagePermission(policyName);
                    return managePermission != null &&
                           context.User.HasClaim(AppPermissions.ClaimType, managePermission);
                })
                .Build();
        }

        return null;
    }
}
