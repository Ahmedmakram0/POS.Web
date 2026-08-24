using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Sales;

public record SaleItemRequest(int ProductId, int Quantity, DiscountType? DiscountType, decimal DiscountValue);

public record SalePaymentRequest(PaymentMethod Method, decimal Amount);

public record CreateSaleRequest(int? CustomerId, List<SaleItemRequest> Items, List<SalePaymentRequest> Payments);

public record SaleListItemDto(
    int Id, string InvoiceNumber, DateTime CreatedAt, string? CustomerName,
    decimal Total, SaleStatus Status, bool HasMinimumPriceOverride);

public interface ISaleService
{
    Task<List<Sale>> GetAllAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
    Task<List<SaleListItemDto>> GetAllForListAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
    Task<Sale?> GetByIdAsync(int id);

    /// <summary>Prices every line, deducts stock, records payments, and posts the resulting cash/credit movements. Runs in a single transaction.</summary>
    Task<Sale> CreateAsync(CreateSaleRequest request, string cashierUserId);

    /// <summary>Voids a completed sale: restocks every line and reverses its financial postings.</summary>
    Task<Sale> VoidAsync(int saleId, string voidedByUserId, string reason);
}
