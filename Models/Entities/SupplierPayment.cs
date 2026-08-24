using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class SupplierPayment
{
    public int Id { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }

    public string PaidByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
