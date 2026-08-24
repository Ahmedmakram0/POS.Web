using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class Sale
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public string CashierUserId { get; set; } = string.Empty;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public decimal SubtotalBeforeDiscount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal Total { get; set; }

    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public decimal CreditAmount { get; set; }

    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    public string? VoidedByUserId { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
