using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models.Enums;
using POS.Web.Services.Catalog;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class CategoriesController(ICategoryService categoryService) : Controller
{
    public async Task<IActionResult> Index(bool includeInactive = false)
    {
        ViewData["IncludeInactive"] = includeInactive;
        return View(await categoryService.GetAllForListAsync(includeInactive));
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await categoryService.CreateAsync(new CreateCategoryRequest(model.Name));
        TempData["Success"] = "تم إضافة الفئة بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await categoryService.GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel { Id = category.Id, Name = category.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await categoryService.UpdateAsync(id, new UpdateCategoryRequest(model.Name));
        TempData["Success"] = "تم تحديث الفئة بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, EntityStatus currentStatus)
    {
        var newStatus = currentStatus == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        await categoryService.SetStatusAsync(id, newStatus);
        TempData["Success"] = "تم تحديث حالة الفئة.";
        return RedirectToAction(nameof(Index));
    }
}
