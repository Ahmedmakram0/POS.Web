using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models.Identity;

namespace POS.Web.Authorization;

/// <summary>
/// Restricts non-admin users (Cashiers) to the controllers listed in their AllowedPages.
/// SuperAdmin/Admin always pass through; controllers outside the restrictable catalog
/// (Home, Account, Users, Settings) are unaffected and rely on their own [Authorize] attributes.
/// </summary>
public class PageAccessFilter(UserManager<ApplicationUser> userManager) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;
        if (httpUser.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();
        if (controller is null || !PageAccess.RestrictableControllers.Contains(controller))
        {
            return;
        }

        if (httpUser.IsInRole("SuperAdmin") || httpUser.IsInRole("Admin"))
        {
            return;
        }

        var user = await userManager.GetUserAsync(httpUser);
        var allowed = PageAccess.Parse(user?.AllowedPages);
        if (!allowed.Contains(controller, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
