using POS.Web.Models.Entities;
using POS.Web.Models.Enums;

namespace POS.Web.Services.Purchasing;

public record PurchaseItemRequest(int ProductId, int Quantity, decimal UnitCostPrice);

public record CreatePurchaseInvoiceRequest(
    int SupplierId,
    List<PurchaseItemRequest> Items,
    decimal AmountPaidNow,
    PaymentMethod? PaymentMethod,
    string? SupplierInvoiceReference = null);

public record PurchaseInvoiceListItemDto(
    int Id, string InvoiceNumber, string? SupplierInvoiceReference, string? SupplierName,
    DateTime CreatedAt, decimal Total, decimal AmountPaid, decimal OutstandingAmount, PurchaseInvoiceStatus Status);

public interface IPurchaseInvoiceService
{
    Task<List<PurchaseInvoice>> GetAllAsync(int? supplierId = null, PurchaseInvoiceStatus? status = null);
    Task<List<PurchaseInvoiceListItemDto>> GetAllForListAsync(int? supplierId = null, PurchaseInvoiceStatus? status = null);
    Task<PurchaseInvoice?> GetByIdAsync(int id);

    /// <summary>Creates the invoice, receives stock for every line, updates product cost prices, and posts any immediate payment.</summary>
    Task<PurchaseInvoice> CreateAsync(CreatePurchaseInvoiceRequest request, string createdByUserId);

    Task<SupplierPayment> AddPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, string paidByUserId);
}
