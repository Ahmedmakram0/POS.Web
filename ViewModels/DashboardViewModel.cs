namespace POS.Web.ViewModels;

public record DailySalesPoint(DateOnly Date, decimal Total);

public record TopProductInsight(string Name, int QuantitySold, decimal Revenue);

public record StockAlertInsight(int ProductId, string Name, int StockQuantity, int MinimumStockLevel)
{
    public bool IsOutOfStock => StockQuantity <= 0;
}

public class DashboardViewModel
{
    public decimal TodaySalesTotal { get; set; }
    public int TodayTransactionsCount { get; set; }
    public decimal MonthSalesTotal { get; set; }

    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }

    public int TotalCustomers { get; set; }

    public decimal SafeBalance { get; set; }
    public decimal InstaPayBalance { get; set; }
    public decimal VodafoneCashBalance { get; set; }
    public decimal CustomerReceivablesBalance { get; set; }
    public decimal SupplierPayablesBalance { get; set; }

    public List<DailySalesPoint> SalesTrend { get; set; } = new();
    public List<TopProductInsight> TopProducts { get; set; } = new();
    public List<StockAlertInsight> StockAlerts { get; set; } = new();
}
