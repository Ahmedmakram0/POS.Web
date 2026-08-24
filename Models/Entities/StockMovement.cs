using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public StockMovementType Type { get; set; }

    // Positive for increases, negative for decreases.
    public int QuantityChange { get; set; }

    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }

    // Free-text reference to the source record (e.g. "Sale#123", "PurchaseInvoice#45").
    public string? Reference { get; set; }
    public string? Reason { get; set; }

    public string PerformedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
