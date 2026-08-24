using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models.Enums;
using POS.Web.Services.Catalog;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class StoresController(IStoreService storeService) : Controller
{
    public async Task<IActionResult> Index(bool includeInactive = false)
    {
        ViewData["IncludeInactive"] = includeInactive;
        return View(await storeService.GetAllAsync(includeInactive));
    }

    [HttpGet]
    public IActionResult Create() => View(new StoreFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StoreFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await storeService.CreateAsync(new CreateStoreRequest(model.Name, model.Address, model.Phone));
        TempData["Success"] = "تم إضافة الفرع بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var store = await storeService.GetByIdAsync(id);
        if (store is null)
        {
            return NotFound();
        }

        return View(new StoreFormViewModel { Id = store.Id, Name = store.Name, Address = store.Address, Phone = store.Phone });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StoreFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await storeService.UpdateAsync(id, new UpdateStoreRequest(model.Name, model.Address, model.Phone));
        TempData["Success"] = "تم تحديث بيانات الفرع.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, EntityStatus currentStatus)
    {
        var newStatus = currentStatus == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        await storeService.SetStatusAsync(id, newStatus);
        TempData["Success"] = "تم تحديث حالة الفرع.";
        return RedirectToAction(nameof(Index));
    }
}
