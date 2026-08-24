using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class PurchaseInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    // The supplier's own invoice/reference number, for reconciling against their paperwork.
    public string? SupplierInvoiceReference { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }

    public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    public ICollection<SupplierPayment> Payments { get; set; } = new List<SupplierPayment>();
}
