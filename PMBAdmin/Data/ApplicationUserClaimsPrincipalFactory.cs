using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PMBAdmin.Authorization;

namespace PMBAdmin.Data;

public class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
            identity.AddClaim(new Claim("DisplayName", user.DisplayName));

        identity.AddClaim(new Claim(AppPermissions.SuperUserClaimType, user.SuperUser.ToString()));
        identity.AddClaim(new Claim("ViewAllAgencies", user.ViewAllAgencies.ToString()));

        var twoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
        identity.AddClaim(new Claim("TwoFactorEnabled", twoFactorEnabled.ToString()));

        return identity;
    }
}
