namespace POS.Web.Models.Entities;

public class ProductPriceHistory
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MinimumSellingPrice { get; set; }

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
