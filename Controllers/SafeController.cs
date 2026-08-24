using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Authorization;
using POS.Web.Models.Enums;
using POS.Web.Services.Financial;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class SafeController(IFinancialAccountService financialAccountService) : Controller
{

    public async Task<IActionResult> Index(FinancialAccountType? accountType, DateTime? from, DateTime? to)
    {
        ViewData["Accounts"] = await financialAccountService.GetAllAccountsAsync();
        ViewData["AccountType"] = accountType;
        ViewData["From"] = from?.ToString("yyyy-MM-dd");
        ViewData["To"] = to?.ToString("yyyy-MM-dd");

        var fromUtc = from?.Date.ToUniversalTime();
        var toUtc = to?.Date.AddDays(1).ToUniversalTime();
        return View(await financialAccountService.GetTransactionsAsync(accountType, fromUtc, toUtc));
    }

    [HttpGet]
    public IActionResult Deposit() => View(new SafeTransactionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(SafeTransactionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await financialAccountService.PostAsync(
                model.AccountType, FinancialTransactionType.ManualDeposit, TransactionDirection.In,
                model.Amount, this.GetCurrentUserId(), description: model.Description);
            TempData["Success"] = "تم تسجيل الإيداع بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Withdraw() => View(new SafeTransactionFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(SafeTransactionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var balance = await financialAccountService.GetBalanceAsync(model.AccountType);
        if (model.Amount > balance)
        {
            ModelState.AddModelError(string.Empty, $"الرصيد الحالي ({balance:N2}) أقل من مبلغ السحب.");
            return View(model);
        }

        try
        {
            await financialAccountService.PostAsync(
                model.AccountType, FinancialTransactionType.ManagerWithdrawal, TransactionDirection.Out,
                model.Amount, this.GetCurrentUserId(), description: model.Description);
            TempData["Success"] = "تم تسجيل السحب بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
