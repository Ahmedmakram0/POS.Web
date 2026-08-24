namespace POS.Web.ViewModels;

public record StockAlertNotification(int ProductId, string Name, int StockQuantity, int MinimumStockLevel)
{
    public bool IsOutOfStock => StockQuantity <= 0;
}

public class StockAlertsViewModel
{
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<StockAlertNotification> Items { get; set; } = new();
    public int TotalCount => LowStockCount + OutOfStockCount;
}
