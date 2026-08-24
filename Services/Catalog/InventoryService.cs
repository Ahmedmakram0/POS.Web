using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public class InventoryService(ApplicationDbContext db) : IInventoryService
{
    public async Task<StockMovement> AdjustStockAsync(
        int productId,
        int quantityChange,
        StockMovementType type,
        string performedByUserId,
        string? reference = null,
        string? reason = null)
    {
        if (quantityChange == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityChange), "Quantity change cannot be zero.");
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        var before = product.StockQuantity;
        var after = before + quantityChange;
        if (after < 0)
        {
            throw new InvalidOperationException(
                $"Stock adjustment would drive '{product.Name}' below zero (before: {before}, change: {quantityChange}).");
        }

        product.StockQuantity = after;
        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = productId,
            Type = type,
            QuantityChange = quantityChange,
            QuantityBefore = before,
            QuantityAfter = after,
            Reference = reference,
            Reason = reason,
            PerformedByUserId = performedByUserId,
        };

        db.StockMovements.Add(movement);
        await db.SaveChangesAsync();
        return movement;
    }

    public async Task<List<StockMovement>> GetMovementsForProductAsync(int productId) =>
        await db.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
}
