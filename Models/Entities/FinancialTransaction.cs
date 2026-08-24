using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public class FinancialTransaction
{
    public int Id { get; set; }

    public int FinancialAccountId { get; set; }
    public FinancialAccount? FinancialAccount { get; set; }

    public FinancialTransactionType Type { get; set; }
    public TransactionDirection Direction { get; set; }
    public decimal Amount { get; set; }

    // Free-text reference to the source record (e.g. "Sale#123", "PurchaseInvoice#45", "Withdrawal").
    public string? Reference { get; set; }
    public string? Description { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
