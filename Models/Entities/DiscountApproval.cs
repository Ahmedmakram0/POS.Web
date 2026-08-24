namespace POS.Web.Models.Entities;

public class DiscountApproval
{
    public int Id { get; set; }

    public int SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }

    public string RequestedByUserId { get; set; } = string.Empty;
    public string ApprovedByUserId { get; set; } = string.Empty;

    public decimal OriginalUnitPrice { get; set; }
    public decimal MinimumSellingPrice { get; set; }
    public decimal RequestedFinalUnitPrice { get; set; }
    public decimal ApprovedFinalUnitPrice { get; set; }

    public bool IsMinimumPriceOverride { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
