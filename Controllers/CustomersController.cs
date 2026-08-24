using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Authorization;
using POS.Web.Models.Enums;
using POS.Web.Services.Customers;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class CustomersController(ICustomerService customerService) : Controller
{

    public async Task<IActionResult> Index(string? search, bool includeInactive = false)
    {
        ViewData["Search"] = search;
        ViewData["IncludeInactive"] = includeInactive;
        return View(await customerService.GetAllAsync(includeInactive, search));
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        ViewData["Balance"] = await customerService.GetBalanceAsync(id);
        ViewData["History"] = await customerService.GetCreditHistoryAsync(id);
        return View(customer);
    }

    [HttpGet]
    public IActionResult Create() => View(new CustomerFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await customerService.CreateAsync(new CreateCustomerRequest(model.Name, model.Phone, model.Address, model.Notes));
        TempData["Success"] = "تم إضافة العميل بنجاح.";
        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(new CustomerFormViewModel
        {
            Id = customer.Id, Name = customer.Name, Phone = customer.Phone, Address = customer.Address, Notes = customer.Notes,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await customerService.UpdateAsync(id, new UpdateCustomerRequest(model.Name, model.Phone, model.Address, model.Notes));
        TempData["Success"] = "تم تحديث بيانات العميل.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, EntityStatus currentStatus)
    {
        var newStatus = currentStatus == EntityStatus.Active ? EntityStatus.Inactive : EntityStatus.Active;
        await customerService.SetStatusAsync(id, newStatus);
        TempData["Success"] = "تم تحديث حالة العميل.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> RecordPayment(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(new CustomerPaymentFormViewModel
        {
            CustomerId = id, CustomerName = customer.Name, CurrentBalance = await customerService.GetBalanceAsync(id),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(CustomerPaymentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await customerService.RecordPaymentAsync(model.CustomerId, model.Amount, model.Method, this.GetCurrentUserId());
            TempData["Success"] = "تم تسجيل الدفعة بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.CustomerId });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
