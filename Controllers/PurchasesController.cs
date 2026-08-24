using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Services.Catalog;
using POS.Web.Services.Purchasing;
using POS.Web.ViewModels;
using POS.Web.Authorization;

namespace POS.Web.Controllers;

[Authorize]
public class PurchasesController(
    IPurchaseInvoiceService purchaseInvoiceService, ISupplierService supplierService, ICategoryService categoryService, IStoreService storeService)
    : Controller
{

    public async Task<IActionResult> Index() => View(await purchaseInvoiceService.GetAllForListAsync());

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
            Items = new List<PurchaseInvoiceItemFormViewModel> { new() },
        };
        await PopulateFormLookupsAsync(model);
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
            await PopulateFormLookupsAsync(model);
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

            var invoice = await purchaseInvoiceService.CreateAsync(request, this.GetCurrentUserId());
            TempData["Success"] = "تم إنشاء فاتورة الشراء بنجاح.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateFormLookupsAsync(model);
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
            await purchaseInvoiceService.AddPaymentAsync(model.InvoiceId, model.Amount, model.Method, this.GetCurrentUserId());
            TempData["Success"] = "تم تسجيل الدفعة بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private async Task PopulateFormLookupsAsync(PurchaseInvoiceFormViewModel model)
    {
        model.Suppliers = await supplierService.GetAllAsync();
        model.Categories = await categoryService.GetAllAsync();
        model.Stores = await storeService.GetAllAsync();
    }
}
