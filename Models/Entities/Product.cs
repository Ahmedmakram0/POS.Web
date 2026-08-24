using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? StoreId { get; set; }
    public Store? Store { get; set; }

    // Physical location within the store (e.g. "Aisle 3, Shelf B").
    public string? Location { get; set; }

    // Cloudinary-hosted product photo. ImagePublicId is kept so the old asset can be
    // deleted from Cloudinary when the image is replaced.
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MinimumSellingPrice { get; set; }

    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }

    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = new List<ProductPriceHistory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
