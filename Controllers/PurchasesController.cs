using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Services.Catalog;
using POS.Web.Services.Purchasing;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class PurchasesController(
    IPurchaseInvoiceService purchaseInvoiceService, ISupplierService supplierService, ICategoryService categoryService, IStoreService storeService)
    : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index() => View(await purchaseInvoiceService.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await purchaseInvoiceService.GetByIdAsync(id);
        return invoice is null ? NotFound() : View(invoice);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? supplierId)
    {
        var model = new PurchaseInvoiceFormViewModel
        {
            SupplierId = supplierId ?? 0,
            Suppliers = await supplierService.GetAllAsync(),
            Categories = await categoryService.GetAllAsync(),
            Stores = await storeService.GetAllAsync(),
            Items = new List<PurchaseInvoiceItemFormViewModel> { new() },
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseInvoiceFormViewModel model)
    {
        var items = model.Items.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList();
        if (items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "يجب إضافة صنف واحد على الأقل (تأكد من مسح أو إدخال كود صحيح لكل صنف).");
        }

        if (!ModelState.IsValid)
        {
            model.Suppliers = await supplierService.GetAllAsync();
            model.Categories = await categoryService.GetAllAsync();
            model.Stores = await storeService.GetAllAsync();
            if (model.Items.Count == 0)
            {
                model.Items.Add(new PurchaseInvoiceItemFormViewModel());
            }
            return View(model);
        }

        try
        {
            var request = new CreatePurchaseInvoiceRequest(
                model.SupplierId,
                items.Select(i => new PurchaseItemRequest(i.ProductId, i.Quantity, i.UnitCostPrice)).ToList(),
                model.AmountPaidNow,
                model.PaymentMethod,
                model.SupplierInvoiceReference);

            var invoice = await purchaseInvoiceService.CreateAsync(request, CurrentUserId);
            TempData["Success"] = "تم إنشاء فاتورة الشراء بنجاح.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Suppliers = await supplierService.GetAllAsync();
            model.Categories = await categoryService.GetAllAsync();
            model.Stores = await storeService.GetAllAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddPayment(int id)
    {
        var invoice = await purchaseInvoiceService.GetByIdAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        return View(new SupplierPaymentFormViewModel
        {
            InvoiceId = invoice.Id, InvoiceNumber = invoice.InvoiceNumber, Outstanding = invoice.OutstandingAmount,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(SupplierPaymentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await purchaseInvoiceService.AddPaymentAsync(model.InvoiceId, model.Amount, model.Method, CurrentUserId);
            TempData["Success"] = "تم تسجيل الدفعة بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
