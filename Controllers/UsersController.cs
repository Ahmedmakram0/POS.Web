using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Authorization;
using POS.Web.Data;
using POS.Web.Models.Identity;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = userManager.Users.OrderBy(u => u.FullName).ToList();
        var items = new List<UserListItemDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(new UserListItemDto(user.Id, user.FullName, user.Email, string.Join(", ", roles), user.IsSuspended));
        }
        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View(new UserCreateViewModel { AvailableRoles = SeedData.Roles.ToList(), AvailablePages = PageAccess.Pages });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        model.AvailableRoles = SeedData.Roles.ToList();
        model.AvailablePages = PageAccess.Pages;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            MaxDiscountPercent = model.MaxDiscountPercent,
            CanDiscountToMinimumPrice = model.CanDiscountToMinimumPrice,
            CanOverrideMinimumPrice = model.CanOverrideMinimumPrice,
            AllowedPages = PageAccess.Serialize(model.AllowedPages),
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        if (await roleManager.RoleExistsAsync(model.Role))
        {
            await userManager.AddToRoleAsync(user, model.Role);
        }

        TempData["Success"] = "تم إضافة المستخدم بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        return View(new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "Cashier",
            MaxDiscountPercent = user.MaxDiscountPercent,
            CanDiscountToMinimumPrice = user.CanDiscountToMinimumPrice,
            CanOverrideMinimumPrice = user.CanOverrideMinimumPrice,
            AllowedPages = PageAccess.Parse(user.AllowedPages),
            AvailableRoles = SeedData.Roles.ToList(),
            AvailablePages = PageAccess.Pages,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel model)
    {
        model.AvailableRoles = SeedData.Roles.ToList();
        model.AvailablePages = PageAccess.Pages;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = model.FullName;
        user.MaxDiscountPercent = model.MaxDiscountPercent;
        user.CanDiscountToMinimumPrice = model.CanDiscountToMinimumPrice;
        user.CanOverrideMinimumPrice = model.CanOverrideMinimumPrice;
        user.AllowedPages = PageAccess.Serialize(model.AllowedPages);
        await userManager.UpdateAsync(user);

        var currentRoles = await userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (await roleManager.RoleExistsAsync(model.Role))
            {
                await userManager.AddToRoleAsync(user, model.Role);
            }
        }

        TempData["Success"] = "تم تحديث بيانات المستخدم.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSuspend(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsSuspended = !user.IsSuspended;
        await userManager.UpdateAsync(user);
        TempData["Success"] = "تم تحديث حالة المستخدم.";
        return RedirectToAction(nameof(Index));
    }
}
