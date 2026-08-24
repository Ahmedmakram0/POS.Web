using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string ProductBarcodeSnapshot { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal OriginalUnitPrice { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmountPerUnit { get; set; }
    public decimal FinalUnitPrice { get; set; }

    public decimal MinimumSellingPriceSnapshot { get; set; }
    public bool MinimumPriceOverridden { get; set; }

    public decimal LineTotal { get; set; }

    public string? ApprovedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
