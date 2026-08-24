using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models.Enums;
using POS.Web.Services.Purchasing;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class SuppliersController(ISupplierService supplierService, IPurchaseInvoiceService purchaseInvoiceService) : Controller
{
    public async Task<IActionResult> Index(string? search, bool includeInactive = false)
    {
        ViewData["Search"] = search;
        ViewData["IncludeInactive"] = includeInactive;
        return View(await supplierService.GetAllAsync(includeInactive, search));
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await supplierService.GetByIdAsync(id);
        if (supplier is null)
        {
            return NotFound();
        }

        ViewData["OutstandingBalance"] = await supplierService.GetOutstandingBalanceAsync(id);
        ViewData["Invoices"] = await purchaseInvoiceService.GetAllAsync(supplierId: id);
        return View(supplier);
    }

    [HttpGet]
    public IActionResult Create() => View(new SupplierFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var supplier = await supplierService.CreateAsync(model.Name, model.Phone, model.Email, model.Address, model.Notes);
        TempData["Success"] = "تم إضافة المورد بنجاح.";
        return RedirectToAction(nameof(Details), new { id = supplier.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await supplierService.GetByIdAsync(id);
        if (supplier is null)
        {
            return NotFound();
        }

        return View(new SupplierFormViewModel
        {
            Id = supplier.Id, Name = supplier.Name, Phone = supplier.Phone, Email = supplier.Email,
            Address = supplier.Address, Notes = supplier.Notes,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await supplierService.UpdateAsync(id, model.Name, model.Phone, model.Email, model.Address, model.Notes);
        TempData["Success"] = "تم تحديث بيانات المورد.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, EntityStatus currentStatus)
    {
        var newStatus = currentStatus == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        await supplierService.SetStatusAsync(id, newStatus);
        TempData["Success"] = "تم تحديث حالة المورد.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
