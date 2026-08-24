using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models;
using POS.Web.Models.Enums;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var trendStart = today.AddDays(-13);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todaySales = await _db.Sales
            .Where(s => s.CreatedAt >= today && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .ToListAsync();

        var monthTotal = await _db.Sales
            .Where(s => s.CreatedAt >= monthStart && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .SumAsync(s => s.Total);

        var trendRows = await _db.Sales
            .Where(s => s.CreatedAt >= trendStart && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .Select(s => new { s.CreatedAt, s.Total })
            .ToListAsync();
        var trendByDay = trendRows.GroupBy(s => DateOnly.FromDateTime(s.CreatedAt)).ToDictionary(g => g.Key, g => g.Sum(x => x.Total));
        var salesTrend = Enumerable.Range(0, 14)
            .Select(offset => DateOnly.FromDateTime(trendStart.AddDays(offset)))
            .Select(date => new DailySalesPoint(date, trendByDay.GetValueOrDefault(date)))
            .ToList();

        var topProducts = (await _db.SaleItems
            .Where(si => si.Sale!.CreatedAt >= monthStart && si.Sale.CreatedAt < tomorrow && si.Sale.Status == SaleStatus.Completed)
            .GroupBy(si => si.ProductNameSnapshot)
            .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.LineTotal) })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync())
            .Select(x => new TopProductInsight(x.Name, x.Quantity, x.Revenue))
            .ToList();

        var stockAlerts = await _db.Products
            .Where(p => p.Status == EntityStatus.Active && p.StockQuantity <= p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(8)
            .Select(p => new StockAlertInsight(p.Id, p.Name, p.StockQuantity, p.MinimumStockLevel))
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TodaySalesTotal = todaySales.Sum(s => s.Total),
            TodayTransactionsCount = todaySales.Count,
            MonthSalesTotal = monthTotal,
            TotalProducts = await _db.Products.CountAsync(p => p.Status == EntityStatus.Active),
            LowStockProducts = await _db.Products.CountAsync(p => p.Status == EntityStatus.Active && p.StockQuantity <= p.MinimumStockLevel && p.StockQuantity > 0),
            OutOfStockProducts = await _db.Products.CountAsync(p => p.Status == EntityStatus.Active && p.StockQuantity <= 0),
            TotalCustomers = await _db.Customers.CountAsync(c => c.Status == EntityStatus.Active),
            SafeBalance = await _db.FinancialAccounts.Where(a => a.Type == FinancialAccountType.CashSafe).Select(a => a.Balance).FirstOrDefaultAsync(),
            InstaPayBalance = await _db.FinancialAccounts.Where(a => a.Type == FinancialAccountType.InstaPay).Select(a => a.Balance).FirstOrDefaultAsync(),
            VodafoneCashBalance = await _db.FinancialAccounts.Where(a => a.Type == FinancialAccountType.VodafoneCash).Select(a => a.Balance).FirstOrDefaultAsync(),
            CustomerReceivablesBalance = await _db.FinancialAccounts.Where(a => a.Type == FinancialAccountType.CustomerReceivables).Select(a => a.Balance).FirstOrDefaultAsync(),
            SupplierPayablesBalance = await _db.FinancialAccounts.Where(a => a.Type == FinancialAccountType.SupplierPayables).Select(a => a.Balance).FirstOrDefaultAsync(),

            SalesTrend = salesTrend,
            TopProducts = topProducts,
            StockAlerts = stockAlerts,
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
