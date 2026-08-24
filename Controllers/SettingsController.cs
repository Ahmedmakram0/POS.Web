using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models.Entities;
using POS.Web.Services.Settings;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class SettingsController(ISettingsService settingsService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new SettingsViewModel
        {
            System = await settingsService.GetSystemSettingsAsync(),
            Receipt = await settingsService.GetReceiptSettingsAsync(),
            Discount = await settingsService.GetDiscountSettingsAsync(),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSystem(SystemSettings model)
    {
        await settingsService.SaveSystemSettingsAsync(model);
        TempData["Success"] = "تم حفظ إعدادات المتجر.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReceipt(ReceiptSettings model, IFormFile? logoFile)
    {
        var error = await settingsService.SaveReceiptSettingsAsync(model, logoFile);
        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "تم حفظ إعدادات الفاتورة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDiscount(DiscountSettings model)
    {
        await settingsService.SaveDiscountSettingsAsync(model);
        TempData["Success"] = "تم حفظ إعدادات الخصومات.";
        return RedirectToAction(nameof(Index));
    }
}
