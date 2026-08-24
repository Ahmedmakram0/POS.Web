using POS.Web.Models.Enums;

namespace POS.Web.Models.Entities;

public enum CustomerCreditTransactionType
{
    CreditSale,
    Payment
}

public class CustomerCreditTransaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public CustomerCreditTransactionType Type { get; set; }

    // Positive amount; Type determines whether it increases or decreases the balance.
    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    // Set when Type == CreditSale.
    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    // Set when Type == Payment.
    public PaymentMethod? PaymentMethod { get; set; }

    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
