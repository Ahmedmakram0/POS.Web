using Microsoft.EntityFrameworkCore;
using POS.Web.Data;
using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Catalog;

public class ProductService(ApplicationDbContext db) : IProductService
{
    public async Task<List<Product>> GetAllAsync(ProductFilter? filter = null)
    {
        filter ??= new ProductFilter();

        var query = db.Products.Include(p => p.Category).Include(p => p.Supplier).Include(p => p.Store).AsQueryable();

        if (!filter.IncludeInactive)
        {
            query = query.Where(p => p.Status == EntityStatus.Active);
        }

        if (filter.CategoryId is int categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (filter.StoreId is int storeId)
        {
            query = query.Where(p => p.StoreId == storeId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                p.Barcode.Contains(term) ||
                (p.SKU != null && p.SKU.Contains(term)));
        }

        if (filter.OnlyLowStock)
        {
            query = query.Where(p => p.StockQuantity <= p.MinimumStockLevel);
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public Task<Product?> GetByIdAsync(int id) =>
        db.Products.Include(p => p.Category).Include(p => p.Supplier).Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetByBarcodeAsync(string barcode) =>
        db.Products.Include(p => p.Category).Include(p => p.Store).FirstOrDefaultAsync(p => p.Barcode == barcode);

    public async Task<Product> CreateAsync(ProductCreateRequest request, string createdByUserId)
    {
        if (await db.Products.AnyAsync(p => p.Barcode == request.Barcode))
        {
            throw new InvalidOperationException($"Barcode '{request.Barcode}' is already in use.");
        }

        var product = new Product
        {
            Barcode = request.Barcode,
            SKU = request.SKU,
            Name = request.Name,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            StoreId = request.StoreId,
            Location = request.Location,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            MinimumSellingPrice = request.MinimumSellingPrice,
            StockQuantity = request.InitialStockQuantity,
            MinimumStockLevel = request.MinimumStockLevel,
            ImageUrl = request.ImageUrl,
            ImagePublicId = request.ImagePublicId,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        db.ProductPriceHistories.Add(new ProductPriceHistory
        {
            ProductId = product.Id,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            MinimumSellingPrice = product.MinimumSellingPrice,
            ChangedByUserId = createdByUserId,
            ChangeReason = "إنشاء المنتج",
        });

        if (request.InitialStockQuantity > 0)
        {
            db.StockMovements.Add(new StockMovement
            {
                ProductId = product.Id,
                Type = StockMovementType.ManualAdjustment,
                QuantityChange = request.InitialStockQuantity,
                QuantityBefore = 0,
                QuantityAfter = request.InitialStockQuantity,
                Reason = "الرصيد الافتتاحي عند إنشاء المنتج",
                PerformedByUserId = createdByUserId,
            });
        }

        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateDetailsAsync(
        int id, string barcode, string? sku, string name, int categoryId, int? supplierId, int minimumStockLevel,
        int? storeId = null, string? location = null)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (barcode != product.Barcode && await db.Products.AnyAsync(p => p.Barcode == barcode && p.Id != id))
        {
            throw new InvalidOperationException($"Barcode '{barcode}' is already in use.");
        }

        product.Barcode = barcode;
        product.SKU = sku;
        product.Name = name;
        product.CategoryId = categoryId;
        product.SupplierId = supplierId;
        product.MinimumStockLevel = minimumStockLevel;
        product.StoreId = storeId;
        product.Location = location;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdatePricingAsync(
        int id, decimal costPrice, decimal sellingPrice, decimal minimumSellingPrice,
        string changedByUserId, string? changeReason = null)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (product.CostPrice == costPrice && product.SellingPrice == sellingPrice && product.MinimumSellingPrice == minimumSellingPrice)
        {
            return product;
        }

        var now = DateTime.UtcNow;

        var openHistoryEntry = await db.ProductPriceHistories
            .Where(h => h.ProductId == id && h.EffectiveTo == null)
            .OrderByDescending(h => h.EffectiveFrom)
            .FirstOrDefaultAsync();
        if (openHistoryEntry is not null)
        {
            openHistoryEntry.EffectiveTo = now;
        }

        product.CostPrice = costPrice;
        product.SellingPrice = sellingPrice;
        product.MinimumSellingPrice = minimumSellingPrice;
        product.UpdatedAt = now;

        db.ProductPriceHistories.Add(new ProductPriceHistory
        {
            ProductId = id,
            CostPrice = costPrice,
            SellingPrice = sellingPrice,
            MinimumSellingPrice = minimumSellingPrice,
            EffectiveFrom = now,
            ChangedByUserId = changedByUserId,
            ChangeReason = changeReason,
        });

        await db.SaveChangesAsync();
        return product;
    }

    public async Task SetStatusAsync(int id, EntityStatus status)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Status = status;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<Product> UpdateImageAsync(int id, string? imageUrl, string? imagePublicId)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.ImageUrl = imageUrl;
        product.ImagePublicId = imagePublicId;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return product;
    }

    public Task<List<Product>> GetLowStockAsync() =>
        db.Products
            .Where(p => p.Status == EntityStatus.Active && p.StockQuantity <= p.MinimumStockLevel)
            .OrderBy(p => p.Name)
            .ToListAsync();
}
