namespace POS.Web.Models.Entities;

// Singleton row (Id = 1) holding store-wide configuration.
public class SystemSettings
{
    public int Id { get; set; }

    public string StoreName { get; set; } = "المتجر";
    public string? StoreAddress { get; set; }
    public string? StorePhone { get; set; }
    public string Currency { get; set; } = "EGP";

    public string InvoiceNumberPrefix { get; set; } = "INV";
    public int NextInvoiceSequence { get; set; } = 1;

    public string PurchaseInvoiceNumberPrefix { get; set; } = "PUR";
    public int NextPurchaseInvoiceSequence { get; set; } = 1;

    public int DefaultLowStockThreshold { get; set; } = 10;
}
