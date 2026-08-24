using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public interface IInventoryService
{
    /// <summary>Applies a stock change to a product and records the movement. Throws if it would drive stock negative.</summary>
    Task<StockMovement> AdjustStockAsync(
        int productId,
        int quantityChange,
        StockMovementType type,
        string performedByUserId,
        string? reference = null,
        string? reason = null);

    Task<List<StockMovement>> GetMovementsForProductAsync(int productId);
}
