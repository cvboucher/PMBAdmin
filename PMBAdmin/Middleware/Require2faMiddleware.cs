using Microsoft.AspNetCore.Authorization;

namespace PMBAdmin.Middleware;

public class Require2faMiddleware(RequestDelegate next)
{
    private static readonly PathString TwoFactorPath = new("/Account/Manage/TwoFactorAuthentication");

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path;

            // Allow Account pages, static assets, and Blazor infrastructure through
            if (!path.StartsWithSegments("/Account") &&
                !path.StartsWithSegments("/_framework") &&
                !path.StartsWithSegments("/_blazor") &&
                !path.StartsWithSegments("/_content") &&
                !HasFileExtension(path))
            {
                var result = await authorizationService.AuthorizeAsync(context.User, "Require2fa");
                if (!result.Succeeded)
                {
                    context.Response.Redirect(TwoFactorPath);
                    return;
                }
            }
        }

        await next(context);
    }

    private static bool HasFileExtension(PathString path)
    {
        return path.HasValue && Path.HasExtension(path.Value);
    }
}
