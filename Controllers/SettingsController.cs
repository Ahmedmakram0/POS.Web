using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class SettingsController(ApplicationDbContext db, IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedLogoExtensions = [".png", ".jpg", ".jpeg", ".webp", ".svg"];
    public async Task<IActionResult> Index()
    {
        var model = new SettingsViewModel
        {
            System = await db.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings(),
            Receipt = await db.ReceiptSettings.FirstOrDefaultAsync() ?? new ReceiptSettings(),
            Discount = await db.DiscountSettings.FirstOrDefaultAsync() ?? new DiscountSettings(),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSystem(SystemSettings model)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            db.SystemSettings.Add(model);
        }
        else
        {
            settings.StoreName = model.StoreName;
            settings.StoreAddress = model.StoreAddress;
            settings.StorePhone = model.StorePhone;
            settings.Currency = model.Currency;
            settings.DefaultLowStockThreshold = model.DefaultLowStockThreshold;
        }
        await db.SaveChangesAsync();
        TempData["Success"] = "تم حفظ إعدادات المتجر.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReceipt(ReceiptSettings model, IFormFile? logoFile)
    {
        var settings = await db.ReceiptSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new ReceiptSettings();
            db.ReceiptSettings.Add(settings);
        }

        settings.ReceiptTitle = model.ReceiptTitle;
        settings.TaxInfo = model.TaxInfo;
        settings.FooterMessage = model.FooterMessage;
        settings.ReceiptWidthMm = model.ReceiptWidthMm is 58 or 80 ? model.ReceiptWidthMm : 80;

        if (logoFile is { Length: > 0 })
        {
            var extension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            if (!AllowedLogoExtensions.Contains(extension))
            {
                TempData["Error"] = "صيغة الصورة غير مدعومة. الصيغ المسموحة: PNG, JPG, WEBP, SVG.";
                return RedirectToAction(nameof(Index));
            }

            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            if (!string.IsNullOrEmpty(settings.LogoUrl))
            {
                var oldPath = Path.Combine(env.WebRootPath, settings.LogoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            var fileName = $"logo-{Guid.NewGuid():N}{extension}";
            await using (var stream = System.IO.File.Create(Path.Combine(uploadsDir, fileName)))
            {
                await logoFile.CopyToAsync(stream);
            }

            settings.LogoUrl = $"/uploads/{fileName}";
        }

        await db.SaveChangesAsync();
        TempData["Success"] = "تم حفظ إعدادات الفاتورة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDiscount(DiscountSettings model)
    {
        var settings = await db.DiscountSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            db.DiscountSettings.Add(model);
        }
        else
        {
            settings.DefaultCashierMaxDiscountPercent = model.DefaultCashierMaxDiscountPercent;
            settings.RequireManagerApprovalAboveLimit = model.RequireManagerApprovalAboveLimit;
        }
        await db.SaveChangesAsync();
        TempData["Success"] = "تم حفظ إعدادات الخصومات.";
        return RedirectToAction(nameof(Index));
    }
}
