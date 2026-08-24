using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Identity;
using POS.Web.Services.Sales;
using POS.Web.ViewModels;
using POS.Web.Authorization;

namespace POS.Web.Controllers;

[Authorize]
public class SalesController(ISaleService saleService, ApplicationDbContext db, UserManager<ApplicationUser> userManager) : Controller
{

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var fromUtc = from?.Date.ToUniversalTime();
        var toUtc = to?.Date.AddDays(1).ToUniversalTime();
        ViewData["From"] = from?.ToString("yyyy-MM-dd");
        ViewData["To"] = to?.ToString("yyyy-MM-dd");
        return View(await saleService.GetAllAsync(fromUtc, toUtc));
    }

    public async Task<IActionResult> Details(int id)
    {
        var sale = await saleService.GetByIdAsync(id);
        return sale is null ? NotFound() : View(sale);
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await saleService.GetByIdAsync(id);
        if (sale is null)
        {
            return NotFound();
        }

        ViewData["ReceiptSettings"] = await db.ReceiptSettings.FirstOrDefaultAsync() ?? new ReceiptSettings();
        ViewData["SystemSettings"] = await db.SystemSettings.FirstOrDefaultAsync() ?? new SystemSettings();

        var cashier = await userManager.FindByIdAsync(sale.CashierUserId);
        ViewData["CashierName"] = cashier?.FullName ?? sale.CashierUserId;

        return View(sale);
    }

    [HttpGet]
    public async Task<IActionResult> Void(int id)
    {
        var sale = await saleService.GetByIdAsync(id);
        if (sale is null)
        {
            return NotFound();
        }

        return View(new SaleVoidFormViewModel { SaleId = id, InvoiceNumber = sale.InvoiceNumber });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Void(SaleVoidFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await saleService.VoidAsync(model.SaleId, this.GetCurrentUserId(), model.Reason);
            TempData["Success"] = "تم إلغاء الفاتورة بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.SaleId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
