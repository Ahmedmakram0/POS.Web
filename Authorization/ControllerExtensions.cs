using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace POS.Web.Authorization;

public static class ControllerExtensions
{
    public static string GetCurrentUserId(this ControllerBase controller) =>
        controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
