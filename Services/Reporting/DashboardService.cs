using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Enums;
using POS.Web.ViewModels;

namespace POS.Web.Services.Reporting;

public class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var trendStart = today.AddDays(-13);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todaySales = await db.Sales
            .Where(s => s.CreatedAt >= today && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .ToListAsync();

        var monthTotal = await db.Sales
            .Where(s => s.CreatedAt >= monthStart && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .SumAsync(s => s.Total);

        var trendRows = await db.Sales
            .Where(s => s.CreatedAt >= trendStart && s.CreatedAt < tomorrow && s.Status == SaleStatus.Completed)
            .Select(s => new { s.CreatedAt, s.Total })
            .ToListAsync();
        var trendByDay = trendRows.GroupBy(s => DateOnly.FromDateTime(s.CreatedAt)).ToDictionary(g => g.Key, g => g.Sum(x => x.Total));
        var salesTrend = Enumerable.Range(0, 14)
            .Select(offset => DateOnly.FromDateTime(trendStart.AddDays(offset)))
            .Select(date => new DailySalesPoint(date, trendByDay.GetValueOrDefault(date)))
            .ToList();

        var topProducts = (await db.SaleItems
            .Where(si => si.Sale!.CreatedAt >= monthStart && si.Sale.CreatedAt < tomorrow && si.Sale.Status == SaleStatus.Completed)
            .GroupBy(si => si.ProductNameSnapshot)
            .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.LineTotal) })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync())
            .Select(x => new TopProductInsight(x.Name, x.Quantity, x.Revenue))
            .ToList();

        var stockAlerts = await db.Products
            .Where(p => p.Status == EntityStatus.Active && p.StockQuantity <= p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(8)
            .Select(p => new StockAlertInsight(p.Id, p.Name, p.StockQuantity, p.MinimumStockLevel))
            .ToListAsync();

        var balancesByType = await db.FinancialAccounts.ToDictionaryAsync(a => a.Type, a => a.Balance);

        return new DashboardViewModel
        {
            TodaySalesTotal = todaySales.Sum(s => s.Total),
            TodayTransactionsCount = todaySales.Count,
            MonthSalesTotal = monthTotal,
            TotalProducts = await db.Products.CountAsync(p => p.Status == EntityStatus.Active),
            LowStockProducts = await db.Products.CountAsync(p => p.Status == EntityStatus.Active && p.StockQuantity <= p.MinimumStockLevel && p.StockQuantity > 0),
            OutOfStockProducts = await db.Products.CountAsync(p => p.Status == EntityStatus.Active && p.StockQuantity <= 0),
            TotalCustomers = await db.Customers.CountAsync(c => c.Status == EntityStatus.Active),
            SafeBalance = balancesByType.GetValueOrDefault(FinancialAccountType.CashSafe),
            InstaPayBalance = balancesByType.GetValueOrDefault(FinancialAccountType.InstaPay),
            VodafoneCashBalance = balancesByType.GetValueOrDefault(FinancialAccountType.VodafoneCash),
            CustomerReceivablesBalance = balancesByType.GetValueOrDefault(FinancialAccountType.CustomerReceivables),
            SupplierPayablesBalance = balancesByType.GetValueOrDefault(FinancialAccountType.SupplierPayables),

            SalesTrend = salesTrend,
            TopProducts = topProducts,
            StockAlerts = stockAlerts,
        };
    }
}
