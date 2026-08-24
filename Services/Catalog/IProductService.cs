using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public record ProductCreateRequest(
    string Barcode,
    string? SKU,
    string Name,
    int CategoryId,
    int? SupplierId,
    decimal CostPrice,
    decimal SellingPrice,
    decimal MinimumSellingPrice,
    int InitialStockQuantity,
    int MinimumStockLevel,
    int? StoreId = null,
    string? Location = null,
    string? ImageUrl = null,
    string? ImagePublicId = null);

public record ProductFilter(string? Search = null, int? CategoryId = null, bool IncludeInactive = false, bool OnlyLowStock = false, int? StoreId = null);

public record ProductListItemDto(
    int Id, string Barcode, string Name, string? CategoryName, string? StoreName, string? Location,
    decimal SellingPrice, int StockQuantity, int MinimumStockLevel, EntityStatus Status, string? ImageUrl);

public interface IProductService
{
    Task<List<Product>> GetAllAsync(ProductFilter? filter = null);
    Task<List<ProductListItemDto>> GetAllForListAsync(ProductFilter? filter = null);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByBarcodeAsync(string barcode);

    Task<Product> CreateAsync(ProductCreateRequest request, string createdByUserId);

    Task<Product> UpdateDetailsAsync(
        int id, string barcode, string? sku, string name, int categoryId, int? supplierId, int minimumStockLevel,
        int? storeId = null, string? location = null);

    /// <summary>Updates pricing and records a closed-out entry in the product's price history.</summary>
    Task<Product> UpdatePricingAsync(
        int id, decimal costPrice, decimal sellingPrice, decimal minimumSellingPrice,
        string changedByUserId, string? changeReason = null);

    Task SetStatusAsync(int id, EntityStatus status);

    Task<Product> UpdateImageAsync(int id, string? imageUrl, string? imagePublicId);

    Task<List<Product>> GetLowStockAsync();
}
